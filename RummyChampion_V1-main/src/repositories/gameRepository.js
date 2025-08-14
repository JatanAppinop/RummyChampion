const { ObjectId } = require('mongodb');
const Game = require('../models/Game');
const User = require('../models/User')
const Settings = require('../models/Settings')

class gameRepository {

    async createGame(gameData) {
        const game = new Game(gameData);
        return await game.save();
    }

    async getSettings() {
        return await Settings.findOne();
    }

    async updateSettings(updatedData) {
        return await Settings.findOneAndUpdate({}, updatedData, { new: true, upsert: true });
    }

    async getAllGames(query) {
        const { player, game, winner, from, to } = query;

        let filter = {};
        if (player) filter.player = new RegExp(player, 'i');
        if (game) filter.game = new RegExp(game, 'i');
        if (winner) filter.winner = new RegExp(winner, 'i');
        if (from && to) filter.gameStartedDate = { $gte: new Date(from), $lte: new Date(to) };

        return await Game.find(filter).sort({ gameStartedDate: -1 });
    }

    async getUserGames(userId) {
        console.log(userId, ">>>>user");
        return await Game.find({ player: { $all: [userId] } })
            .populate('players')
            .populate('winner')
            .populate('tableId');
    }

    async getGameDetails(gameId) {
        return await Game.findById(gameId)
            .populate({
                path: 'players',
                select: 'mobileNumber username status type verified emailVerified kycVerified mobileVerified refercode cashBonus totalBalance winningBalance depositBalance fcmToken fcmDevice firsttime createdAt updatedAt totalCoins kycSubmitted backdropIndex profilePhotoIndex'
            })
            .populate({
                path: 'tableId',
                select: 'bet totalBet rake rakePercentage wonCoin isActive gameMode gameType createdOn'
            })
            .exec();
    }

    async findById(gameId) {
        return await Game.findById(gameId);
    }

    async findByType(gameType) {
        return await Game.find({ gameType }).populate('players winner');
    }

    async updateWinner(gameId, winnerId, wonCoin) {
        return await Game.findByIdAndUpdate(gameId, {
            winner: winnerId,
            wonCoin,
            gameWonDate: Date.now()
        }, { new: true });
    }

    async updateGame(gameId, updateData) {
        const game = await Game.findByIdAndUpdate(gameId, updateData, { new: true });
        return game;
    };

    async addPlayerToGame(gameId, userId) {
        const game = await Game.findById(gameId);
        if (!game) throw { status: 404, message: 'Game not found' };
        if (game.players.includes(userId)) throw { status: 400, message: 'User already in game' };
        game.players.push(userId);
        await game.save();
        return game;
    };

    async findGameById(gameId) {
        const game = await Game.findById(gameId).populate('players winner');
        return game;
    };

    async findGamesByUserId(userId) {
        const games = await Game.find({ players: userId }).populate('players winner');
        return games;
    };

    async findActiveGames() {
        const games = await Game.find({ winner: { $exists: false } }).populate('players');
        return games;
    };

    async calculateLeaderBoard() {
        try {
            console.log("Starting calculateLeaderBoard..."); // Logging
    
            // Fetch all users with their win counts
            const leaderBoard = await User.aggregate([
                {
                    $lookup: {
                        from: 'games',
                        let: { userId: '$_id' },
                        pipeline: [
                            { $match: { $expr: { $in: ['$$userId', '$winner'] } } },
                            { $group: { _id: null, wins: { $sum: 1 } } },
                            { $project: { _id: 0, wins: 1 } }
                        ],
                        as: 'winCounts'
                    }
                },
                {
                    $addFields: {
                        wins: { $ifNull: [{ $arrayElemAt: ['$winCounts.wins', 0] }, 0] }
                    }
                },
                {
                    $sort: { wins: -1 } // Sort by highest wins
                }
            ]);
    
            // Manually assign ranks
            let rank = 1;
            for (let i = 0; i < leaderBoard.length; i++) {
                if (i > 0 && leaderBoard[i].wins < leaderBoard[i - 1].wins) {
                    rank = i + 1; // Assign new rank if wins decrease
                }
                leaderBoard[i].rank = rank;
            }
    
            console.log("LeaderBoard Result:", leaderBoard); // Logging
            return leaderBoard;
        } catch (error) {
            console.error("Error in calculateLeaderBoard:", error); // Logging
            throw error;
        }
    }
    

    async allGameData() {
        return await Game.find()
            .populate({
                path: 'players',
                select: 'mobileNumber username status type verified emailVerified kycVerified mobileVerified refercode cashBonus totalBalance winningBalance depositBalance fcmToken fcmDevice firsttime createdAt updatedAt totalCoins kycSubmitted backdropIndex profilePhotoIndex'
            })
            .populate({
                path: 'tableId',
                select: 'bet totalBet rake rakePercentage wonCoin isActive gameMode game gameType createdOn'
            })
            .exec();
    }

}

module.exports = new gameRepository()