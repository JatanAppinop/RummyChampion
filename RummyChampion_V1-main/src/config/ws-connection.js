const http = require('http');
const socketIO = require('socket.io');
// const Match = require("../models/Match");

const onlinePlayers = [];
const playersWaitingForMatch = [];

const logOnlinePlayers = () => {
    console.log('Online players:', onlinePlayers);
    console.log('Total online players: ', onlinePlayers.length);
    console.log('Players Searching: ', playersWaitingForMatch.length);
};

function disconnectPlayer(socketId, io) {
    const targetSocket = io.sockets.sockets.get(socketId);
    console.log("Target Socket" + targetSocket)
    if (targetSocket) {
        targetSocket.disconnect(true);
    }
}

const matchPlayers = async (newPlayer, numberOfPlayersToMatch, io) => {
    const matchedPlayers = [];

    // Filter players waiting for a match based on contest ID
    const eligiblePlayers = playersWaitingForMatch.filter(player => player.contestId === newPlayer.contestId);

    if (eligiblePlayers.length >= numberOfPlayersToMatch) {
        // Extract players from waiting list for matching
        for (let i = 0; i < numberOfPlayersToMatch; i++) {
            matchedPlayers.push(eligiblePlayers[i]);
            const index = playersWaitingForMatch.indexOf(eligiblePlayers[i]);
            playersWaitingForMatch.splice(index, 1);
        }

        try {
            // Create a match object
            const match = new Match({
                contestId: newPlayer.contestId,
                playerIds: matchedPlayers.map(player => player.playerId),
                startDateTime: new Date(),
                endDateTime: null, // This will be updated when the match ends
                results: []
            });

            // Save the match object to the database
            await match.save();

            // Emit match ID to matched players
            const matchId = match._id.toString();

            console.log("Match Found : " + matchId)
            logOnlinePlayers();

            matchedPlayers.forEach(player => {
                io.to(player.socketId).emit('match_found', { matchId });
            });

            matchedPlayers.forEach(player => {
                disconnectPlayer(player.socketId, io);
            });
        } catch (error) {
            console.error('Error creating and saving match:', error);
            // Handle error
        }
    }
};

module.exports = {

    inititate: (app) => {
        const server = http.createServer(app);
        const io = socketIO(server, { cors: { origin: '*' } });
        return { server, io };
    },

    connect: async (io) => {

        const GameServerNamespace = io.of('/gameserver');

        GameServerNamespace.on('connection', async (socket) => {
            if (socket.handshake.query.matchId && socket.handshake.query.playerId) {

                try {
                    // 1. Get match data
                    const match = await Match.findById(socket.handshake.query.matchId);

                    if (!match) {
                        throw new Error('Match not found');
                    }
                    if (!match.playerIds.includes(socket.handshake.query.playerId)) {
                        throw new Error('Player not authorized for this match');
                    }
                    if (match.results.length > 0 || match.endDateTime !== null) {
                        throw new Error('Match Already Over');
                    }
                    // 3. Add Player to a Private Room
                    socket.join(match.id);

                    // 4. Create an Array of {playerId, socket.id}
                    const newPlayer = {
                        socketId: socket.id,
                        playerId: socket.handshake.query.playerId,
                        contestId: match.contestId,
                    };
                    onlinePlayers.push(newPlayer);

                    const matchPlayers = onlinePlayers.filter(player => match.playerIds.includes(player.playerId));

                    // 5. Check if all players are connected and fire start match event
                    if (matchPlayers.length === match.playerIds.length) {
                        GameServerNamespace.to(match.id).emit('start_match', {
                            playerId: match.playerIds[0] // Or any other relevant data for the match
                        });
                    }

                    logOnlinePlayers();

                } catch (error) {
                    console.error(error);
                    GameServerNamespace.in(socket.id).disconnectSockets();
                    logOnlinePlayers();
                }
            } else {
                console.error('Missing MatchID or playerID');
                GameServerNamespace.in(socket.id).disconnectSockets();
                logOnlinePlayers();
            }
            // Dice clicked
            socket.on('Dice_Clicked', async (data) => {
                console.log(" Dice Clicked : Room : " + socket.handshake.query.matchId + " Data : " + data);
                GameServerNamespace.to(socket.handshake.query.matchId).emit('Dice_Clicked', data);
            });
            // Dice rolled
            socket.on('Dice_Rolled', async (data) => {
                console.log(" Dice rolled : Room : " + socket.handshake.query.matchId + " Data : " + data);
                GameServerNamespace.to(socket.handshake.query.matchId).emit('Dice_Rolled', data);
            });
            // tokenMoved
            socket.on('MoveToken', async (data) => {
                console.log(" Token Moved : Room : " + socket.handshake.query.matchId + " Data : " + data);
                GameServerNamespace.to(socket.handshake.query.matchId).emit('MoveToken', data);
            });

            // Game Finished
            socket.on('Game_Finished', async (data) => {
                console.log(" Game_Finished : Room : " + socket.handshake.query.matchId + " Data : " + data);

                try {
                    const matchId = socket.handshake.query.matchId;
                    const { playerId, reason } = data;

                    if (!matchId || !playerId || !reason) {
                        throw new Error('Missing required parameters in Game_Finished event data.');
                    }

                    const match = await Match.findById(matchId);

                    if (!match) {
                        throw new Error('Match not found: ' + matchId);
                    }

                    if (match.endDateTime !== null) {
                        throw new Error('Match already has an end date. No need to update.');
                    }

                    if (!match.playerIds.includes(playerId)) {
                        throw new Error('Player is not part of the match.');
                    }

                    const endDate = new Date();
                    match.endDateTime = endDate;

                    if (reason === 'won') {
                        match.results = [playerId, match.playerIds.find(id => id !== playerId)];
                    } else if (reason === 'misstry') {
                        match.results = [match.playerIds.find(id => id !== playerId), playerId];
                    } else {
                        throw new Error('Invalid reason provided in Game_Finished event data.');
                    }

                    await match.save();

                    console.log('Game Finished - Match Updated:', match);
                    GameServerNamespace.to(socket.handshake.query.matchId).emit('Game_Finished', data);

                    // Disconnect users
                    const playerstoDisconnect = match.playerIds.map(id => {
                        const player = onlinePlayers.find(onlinePlayer => onlinePlayer.playerId === id);
                        if (player) {
                            return player.socketId;
                        }
                        return null;
                    }).filter(id => id !== null);

                    playerstoDisconnect.forEach(socketId => {
                        GameServerNamespace.in(socketId).disconnectSockets();
                    });

                } catch (error) {
                    console.error('An error occurred while processing Game_Finished event:', error);
                }


            })


            socket.on('Turn_Missed', async (data) => {
                console.log(" Turn_Missed : Room : " + socket.handshake.query.matchId + " Data : " + data);
                GameServerNamespace.to(socket.handshake.query.matchId).emit('Turn_Missed', data);
            });

            // Turn Finished
            socket.on('Turn_Finished', async (data) => {
                console.log(" Turn_Finished : Room : " + socket.handshake.query.matchId + " Data : " + data);
                GameServerNamespace.to(socket.handshake.query.matchId).emit('NextTurn', data);
            });
            // Match_Cancelled
            socket.on('Match_Cancelled', async (data) => {
                const matchId = socket.handshake.query.matchId;
                const playerId = socket.handshake.query.playerId;
                try {
                    if (matchId) {
                        const match = await Match.findById(matchId);

                        if (!match) {
                            throw new Error('Match not found : ' + matchId);
                        }

                        if (match.endDateTime !== null) {
                            throw new Error('Match already has an end date. No need to update.');
                        }

                        if (!match.playerIds.includes(playerId)) {
                            throw new Error('Unauthorised Player.');
                        }

                        const endDate = new Date();
                        match.endDateTime = endDate;
                        await match.save();

                        const playerstoDisconnect = match.playerIds.map(id => {
                            const player = onlinePlayers.find(onlinePlayer => onlinePlayer.playerId === id);
                            if (player) {
                                return player.socketId; // Get the socket ID of online players
                            }
                            return null;
                        }).filter(id => id !== null);

                        playerstoDisconnect.forEach(socketId => {
                            GameServerNamespace.in(socketId).disconnectSockets(); // Disconnect each online player
                        });

                        console.log('Match Cancelled - End Time Updated:', match);
                    }
                } catch (error) {
                    console.error('An error occurred while processing Match_Cancelled event:', error);
                }

            });

            socket.on('disconnect', async () => {
                const playerId = socket.handshake.query.playerId;
                const matchId = socket.handshake.query.matchId;
                try {
                    if (matchId && playerId) {

                        // Remove player from onlinePlayers
                        const index = onlinePlayers.findIndex(player => player.playerId === playerId);
                        if (index !== -1) {
                            onlinePlayers.splice(index, 1);
                        }

                        const match = await Match.findById(matchId);

                        if (!match) {
                            throw new Error('Match not found : ' + matchId);
                        }
                        if (match.results.length > 0 || match.endDateTime !== null) {
                            throw new Error('Match already has results or an end date. No need to update.');
                        }

                        if (match.playerIds.length === 2) {

                            const winnerId = match.playerIds.find(id => id !== playerId);
                            const endDate = new Date();
                            match.endDateTime = endDate;
                            match.results = [winnerId, playerId];
                            await match.save();

                            // Notify the room about disconnection
                            GameServerNamespace.to(matchId).emit('player_disconnected', { playerId });

                            const playerstoDisconnect = match.playerIds.map(id => {
                                const player = onlinePlayers.find(onlinePlayer => onlinePlayer.playerId === id);
                                if (player) {
                                    return player.socketId; // Get the socket ID of online players
                                }
                                return null;
                            }).filter(id => id !== null);

                            playerstoDisconnect.forEach(socketId => {
                                GameServerNamespace.in(socketId).disconnectSockets(); // Disconnect each online player
                            });

                            console.log('Match Disconnected', match);

                        }

                    }
                } catch (error) {
                    console.error('Player Disconnection Error :', error);
                }

            });

        });

        io.on('connection', async (socket) => {
            if (socket.handshake.query.playerId && socket.handshake.query.contestId) {
                const newPlayer = {
                    socketId: socket.id,
                    playerId: socket.handshake.query.playerId,
                    contestId: socket.handshake.query.contestId,
                };
                console.log('Player connected:', newPlayer.playerId); // Optional logging

                // Check for duplicate players and remove if found
                removePlayerFromLists(newPlayer.playerId);

                onlinePlayers.push(newPlayer);
                // Add player to waiting list for matchmaking
                playersWaitingForMatch.push(newPlayer);

                if (playersWaitingForMatch.length >= 2) {
                    matchPlayers(newPlayer, 2, io);
                }

                logOnlinePlayers();

            } else {
                console.error('Missing playerId or contestId');
                disconnectPlayer(socket.id, io);
                logOnlinePlayers();
            }


            socket.on('message', async (data) => {
                console.log(" Message : " + data);

            });

            socket.on('disconnect', () => {
                const index = onlinePlayers.findIndex(
                    (player) => player.socketId === socket.id
                );
                if (index !== -1) {
                    onlinePlayers.splice(index, 1);
                    console.log('Player disconnected:', socket.id); // Optional logging
                }
                const index2 = playersWaitingForMatch.findIndex(player => player.socketId === socket.id);
                playersWaitingForMatch.splice(index, 1);
                logOnlinePlayers();
            });
        });

        // Helper Function to remove duplicates
        function removePlayerFromLists(playerId) {
            [onlinePlayers, playersWaitingForMatch].forEach(list => {
                const index = list.findIndex(player => player.playerId === playerId);
                if (index !== -1) {
                    const existingPlayer = list[index];
                    disconnectPlayer(existingPlayer.socketId, io); // Disconnect existing socket
                    list.splice(index, 1); // Remove from the list
                }
            });
        }
    }
}