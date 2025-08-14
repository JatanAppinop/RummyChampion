const http = require("http");
const { Server } = require("socket.io");
const mongoose = require("mongoose");
const Game = require("../models/Game");
const Table = require("../models/Table");
const User = require("../models/User");
const OnlinePlayers = require("../models/OnlinePlayers");
const Transaction = require("../models/Transaction");
const axios = require("axios");
const { newDeck, shuffleDeck } = require("../utils/deckCard");

const onlinePlayers = [];
const playersSearching = [];
const turnAcknowledgements = {};
const playerReadiness = {};
const connectedPlayer = {};

const rummyPlayerReadiness = {};
const rummyConnectedPlayers = {};
const currentPlayerTurn = {};
const cardsDealtTracker = {};
const cardCache = {};
const drawPiles = {}; // Custom draw piles per match
const discardPiles = {}; // Custom discard piles per match
const playerHands = {}; // Custom player hands per match
const shuffleConfirmations = {};
// const jokerCard = {};
let dealTracker = {}; // Tracks the number of deals for each match (for Deal Rummy)
let cumulativeScores = {}; // Tracks cumulative scores for each player in a match (for Deal Rummy and Pool Rummy)
let matchWins = {}; // Tracks the number of wins for each player in a match (for Deal Rummy)
let poolScores = {}; // Tracks scores for each player in Pool Rummy
let playerGameData = {}; // 🔹 NEW: Tracks enhanced player data from player_ready events
let eliminatedPlayers = {}; // 🔹 NEW: Tracks eliminated players in Pool Rummy

const fetchCard = async (deckId, numCards = 1) => {
  try {
    const response = await axios.get(
      `https://deckofcardsapi.com/api/deck/${deckId}/draw/?count=${numCards}`
    );
    return response.data.cards;
  } catch (error) {
    console.error("Error fetching card:", error);
    throw new Error("Unable to fetch card from the deck API");
  }
};

module.exports = {
  inititate: (app) => {
    const server = http.createServer(app);
    const io = new Server(server, { cors: { origin: "*" } });
    return { server, io };
  },

  connect: async (io) => {
    const GameServerNamespace = io.of("/gameserver");
    const RummyServerNamespace = io.of("/rummyserver");

    RummyServerNamespace.on("connection", async (socket) => {
      const { matchId, playerId } = socket.handshake.query; // Add gameType to query
      let gameType;
      let playerCount;
      let game;

      if (matchId && playerId) {
        try {
          game = await Game.findById(matchId).populate("tableId");
          if (!game) throw new Error("Game not found");
          if (!game.players.includes(playerId))
            throw new Error("Player not authorized for this game");

          gameType = game.tableId.gameMode;
          console.log("🚀 ~ RummyServerNamespace.on ~ gameType:", gameType);
          playerCount = game.players.length;
          socket.join(game.id);

          const newPlayer = {
            socketId: socket.id,
            playerId,
            contestId: game.tableId._id,
          };
          console.log(playerId, "Connected to RummyServer");
          removeDuplicatePlayer(playerId, io);

          onlinePlayers.push(newPlayer);
          await addPlayerToDB(newPlayer);

          // Initialize game type-specific variables
          if (gameType === "Deals") {
            if (!dealTracker[matchId]) {
              dealTracker[matchId] = 0;
            }
            if (!cumulativeScores[matchId]) {
              cumulativeScores[matchId] = {};
              game.players.forEach((player) => {
                cumulativeScores[matchId][player] = 0;
              });
            }
            if (!matchWins[matchId]) {
              matchWins[matchId] = {};
              game.players.forEach((player) => {
                matchWins[matchId][player] = 0;
              });
              console.log(
                "🚀 ~ RummyServerNamespace.on ~ matchWins[matchId]:",
                matchWins[matchId]
              );
            }
          } else if (gameType === "101 Pool" || gameType === "201 Pool") {
            if (!poolScores[matchId]) {
              poolScores[matchId] = {};
              game.players.forEach((player) => {
                poolScores[matchId][player] = 0; // Initialize scores to 0 for Pool Rummy
              });
            }
          }

          // Initialize readiness tracking for this match
          if (!rummyPlayerReadiness[matchId]) {
            rummyPlayerReadiness[matchId] = new Set();
          }
          if (!rummyConnectedPlayers[matchId]) {
            rummyConnectedPlayers[matchId] = [];
          }
          if (!cardsDealtTracker[matchId]) {
            cardsDealtTracker[matchId] = new Set();
          }
          if (!cardCache[matchId]) {
            cardCache[matchId] = {};
          }
          if (!drawPiles[matchId]) {
            drawPiles[matchId] = [];
          }
          if (!discardPiles[matchId]) {
            discardPiles[matchId] = [];
          }
          if (!playerHands[matchId]) {
            playerHands[matchId] = {};
          }
          if (!shuffleConfirmations[matchId]) {
            shuffleConfirmations[matchId] = new Set();
          }

          // =============== EVENT: player_ready (ENHANCED) =================
          socket.on("player_ready", async (playerReadyData) => {
            try {
              console.log(`🔍 [BACKEND DEBUG] ===== PLAYER_READY EVENT RECEIVED =====`);
              console.log(`🔍 [BACKEND DEBUG] Event: player_ready received from socket: ${socket.id}`);
              console.log(`🔍 [BACKEND DEBUG] Player ID from handshake: ${playerId}`);
              console.log(`🔍 [BACKEND DEBUG] Match ID from handshake: ${matchId}`);
              console.log(`🔍 [BACKEND DEBUG] Data received:`, playerReadyData);
              console.log(`🔍 [BACKEND DEBUG] Data type:`, typeof playerReadyData);
              console.log(`🔍 [BACKEND DEBUG] Data is null:`, playerReadyData === null);
              console.log(`🔍 [BACKEND DEBUG] Data is undefined:`, playerReadyData === undefined);
              
              if (playerReadyData) {
                console.log(`🔍 [BACKEND DEBUG] Data keys:`, Object.keys(playerReadyData));
                console.log(`🔍 [BACKEND DEBUG] Data values:`, Object.values(playerReadyData));
                console.log(`🔍 [BACKEND DEBUG] Received playerId in data: ${playerReadyData.playerId}`);
                console.log(`🔍 [BACKEND DEBUG] Received playerName in data: ${playerReadyData.playerName}`);
                console.log(`🔍 [BACKEND DEBUG] Received matchId in data: ${playerReadyData.matchId}`);
                console.log(`🔍 [BACKEND DEBUG] Received gameMode in data: ${playerReadyData.gameMode}`);
                console.log(`🔍 [BACKEND DEBUG] Received gameType in data: ${playerReadyData.gameType}`);
              }
              
              console.log(`🔍 [BACKEND DEBUG] Player count: ${playerCount}`);
              console.log(`🔍 [BACKEND DEBUG] Game type: ${gameType}`);
              
              if (!playerCount) {
                console.error("❌ [BACKEND DEBUG] Player count is not defined");
                console.log(`🔍 [BACKEND DEBUG] Sending error response to client`);
                socket.emit("player_ready", { 
                  status: "error", 
                  reason: "Player count not defined",
                  nextEvent: "wait_for_players"
                });
                return;
              }

              // Handle enhanced player ready data if provided
              if (playerReadyData && typeof playerReadyData === 'object') {
                console.log(`✅ [Backend] Processing enhanced player data:`, {
                  playerId: playerReadyData.playerId,
                  playerName: playerReadyData.playerName,
                  gameMode: playerReadyData.gameMode,
                  gameType: playerReadyData.gameType,
                  isReady: playerReadyData.isReady
                });
                
                // Validate player data
                if (playerReadyData.playerId !== playerId) {
                  console.warn(`⚠️ [Backend] Player ID mismatch: expected ${playerId}, got ${playerReadyData.playerId}`);
                }
                
                // Store additional player info for game management
                if (!playerGameData[matchId]) {
                  playerGameData[matchId] = {};
                }
                playerGameData[matchId][playerId] = {
                  playerName: playerReadyData.playerName,
                  gameMode: playerReadyData.gameMode,
                  gameType: playerReadyData.gameType,
                  walletBalance: playerReadyData.walletBalance,
                  clientVersion: playerReadyData.clientVersion,
                  deviceInfo: playerReadyData.deviceInfo,
                  readyTime: new Date()
                };
              }

              rummyPlayerReadiness[matchId].add(playerId);
              rummyConnectedPlayers[matchId].push(playerId);

              console.log(`🔍 [BACKEND DEBUG] Players ready: ${rummyPlayerReadiness[matchId].size}/${playerCount}`);
              console.log(`🔍 [BACKEND DEBUG] Ready players list:`, Array.from(rummyPlayerReadiness[matchId]));
              console.log(`🔍 [BACKEND DEBUG] Connected players list:`, rummyConnectedPlayers[matchId]);

              // Send acknowledgment back to client
              const responseData = {
                status: "ready",
                playerId: playerId,
                readyPlayers: rummyPlayerReadiness[matchId].size,
                totalPlayers: playerCount,
                nextEvent: rummyPlayerReadiness[matchId].size === playerCount ? "start_game" : "wait_for_players"
              };
              
              console.log(`🔍 [BACKEND DEBUG] ===== SENDING RESPONSE TO CLIENT =====`);
              console.log(`🔍 [BACKEND DEBUG] Response data:`, responseData);
              console.log(`🔍 [BACKEND DEBUG] Sending to socket ID: ${socket.id}`);
              
              socket.emit("player_ready", responseData);
              
              console.log(`✅ [BACKEND DEBUG] Response sent successfully`);

              if (rummyPlayerReadiness[matchId].size === playerCount) {
                console.log("🚀 [Backend] All players are ready. Starting the Rummy match.");

                // Reset game state for a new deal (only for Deal Rummy)
                if (gameType === "Deals" && dealTracker[matchId] > 0) {
                  playerHands[matchId] = {};
                  drawPiles[matchId] = [];
                  discardPiles[matchId] = [];
                  cardCache[matchId] = {};
                }

                try {
                  const deckCount = playerCount > 2 ? 2 : 1;
                  const drawResponse = newDeck(deckCount);
                  console.log("🃏 [Backend] Created deck:", drawResponse);

                  drawPiles[matchId] = shuffleDeck(drawResponse.cards);
                  console.log("🔀 [Backend] Shuffled deck:", drawPiles[matchId].length, "cards");

                  game.players.forEach((player) => {
                    playerHands[matchId][player] = [];
                  });

                  // Deal 13 cards to each player
                  game.players.forEach((player) => {
                    for (let i = 0; i < 13; i++) {
                      const card = drawPiles[matchId].pop();
                      playerHands[matchId][player].push(card);
                    }
                  });

                  const lastCard = drawPiles[matchId].pop();
                  discardPiles[matchId].push(lastCard);
                  console.log("🎲 [Backend] Dealt cards, remaining in deck:", drawPiles[matchId].length);

                                     RummyServerNamespace.to(matchId).emit("deal_cards", {
                     players: game.players.map((player) => ({
                       playerId: player,
                       cards: playerHands[matchId][player],
                     })),
                     lastCard,
                   });
                 } catch (error) {
                   console.error("❌ [Backend] Error setting up game:", error);
                   socket.emit("player_ready", {
                     status: "error",
                     reason: "Game setup failed",
                     nextEvent: "wait_for_players"
                   });
                 }
               }
             } catch (error) {
               console.error("❌ [Backend] Error in player_ready handler:", error);
               socket.emit("player_ready", {
                 status: "error",
                 reason: "Server error processing ready event",
                 nextEvent: "wait_for_players"
               });
             }
           });

          // =============== EVENT: cards_received =================
          socket.on("cards_received", () => {
            cardsDealtTracker[matchId].add(playerId);

            // Check if all players confirmed
            if (cardsDealtTracker[matchId].size === playerCount) {
              console.log(
                "All players confirmed receipt of cards. Starting game."
              );
              currentPlayerTurn[matchId] = 0;
              const currentPlayerId = game.players[currentPlayerTurn[matchId]];
              RummyServerNamespace.to(matchId).emit("start_turn", {
                playerId: currentPlayerId,
              });
            }
          });

          // =============== EVENT: request_card =================
          socket.on("request_card", async (data) => {
            try {
              const { cardCode } = data;
              console.log("🚀 ~ Current drawPiles:", drawPiles[matchId]);

              // Find the requested card in the draw pile
              const cardIndex = drawPiles[matchId].findIndex(
                (card) => card.code === cardCode
              );

              if (cardIndex === -1) {
                console.log("Card not found in the deck");
                socket.emit("error", { message: "Card not found in the deck" });
                return;
              }

              // Remove the card from the draw pile and add to player's hand
              const card = drawPiles[matchId].splice(cardIndex, 1)[0];
              playerHands[matchId][playerId].push(card);

              console.log("🚀 ~ Card fetched:", card);

              // Notify all players about the updated hand and fetched card
              RummyServerNamespace.to(matchId).emit("update_player_hand", {
                playerId,
                cards: playerHands[matchId][playerId],
              });

              RummyServerNamespace.to(matchId).emit("card_fetched", {
                card,
                requestedBy: playerId,
                drawPiles: drawPiles[matchId],
              });
              console.log(
                "🚀 ~ RummyServerNamespace.to ~ length:",
                discardPiles[matchId].length,
                drawPiles[matchId].length
              );
              // Validate if matchId exists and deck has cards
              if (drawPiles[matchId].length === 1) {
                const lastCard = discardPiles[matchId].pop();
                drawPiles[matchId] = shuffleArray([...discardPiles[matchId]]);
                discardPiles[matchId] = [lastCard];

                console.log("♻️ No cards left! Reshuffling discard pile...");

                RummyServerNamespace.to(matchId).emit("reshuffle", {
                  playerId,
                  drawPiles: drawPiles[matchId],
                  topCard: lastCard,
                });
              }
            } catch (error) {
              console.error("❌ Error handling request_card:", error);
              socket.emit("error", { message: "Unable to fetch card" });
            }
          });

          // =============== EVENT: pick_card =================
          // pick_card
          //   socket.on("pick_card", (data) => {
          //     try {
          //         const { matchId, playerId, cardCode } = data;

          //         if (!matchId || !playerId || !cardCode) {
          //             socket.emit("error", { message: "Invalid request" });
          //             return;
          //         }

          //         // Ensure the card is from a valid source (e.g., the discard pile)
          //         if (!discardPiles[matchId] || !discardPiles[matchId].includes(cardCode)) {
          //             socket.emit("error", { message: "Card not available in discard pile" });
          //             return;
          //         }

          //         // Remove the card from the discard pile
          //         discardPiles[matchId] = discardPiles[matchId].filter(card => card !== cardCode);

          //         // Add card to the player's hand
          //         playerHands[matchId][playerId].push(cardCode);

          //         RummyServerNamespace.to(matchId).emit("card_picked", {
          //             cardCode,
          //             playerId,
          //         });
          //     } catch (error) {
          //         console.error("Error handling pick_card:", error);
          //         socket.emit("error", { message: "Unable to pick card" });
          //     }
          // });

          // =============== EVENT: (Optional) take_card_from_deck ===============
          // Not in use, but kept for reference
          socket.on("take_card_from_deck", () => {
            try {
              const card = cardCache[matchId].cachedCard;
              if (!card) {
                socket.emit("error", { message: "No cached card available" });
                return;
              }

              playerHands[matchId][playerId].push(card);
              delete cardCache[matchId].cachedCard;

              RummyServerNamespace.to(matchId).emit("card_taken_from_deck", {
                playerId,
                cardCode: card.code,
              });
            } catch (error) {
              console.error("Error in take_card_from_deck:", error);
              socket.emit("error", {
                message: "Unable to take card right now",
              });
            }
          });

          // =============== EVENT: drop_card =================
          socket.on("drop_card", (data) => {
            console.log("🚀 ~ socket.on ~ data:", data);
            try {
              const { cardCode } = data;
              console.log("🚀 ~ socket.on ~ cardCode:", cardCode);
              const playerHand = playerHands[matchId][playerId];
              console.log("🚀 ~ socket.on ~ playerHand:", playerHand);

              // Find the index of the card in the player's hand
              const cardIndex = playerHand.findIndex(
                (card) => card.code === cardCode
              );
              console.log("🚀 ~ socket.on ~ cardIndex:", cardIndex);

              if (cardIndex !== -1) {
                // Remove the card from the player's hand
                const [card] = playerHand.splice(cardIndex, 1);

                // Add the card to the discard pile
                discardPiles[matchId].push(card);

                // Emit the card_dropped event with the card details
                RummyServerNamespace.to(matchId).emit("card_dropped", {
                  playerId,
                  cardCode,
                  discardPile: discardPiles[matchId],
                });

                // Emit an update to the player's hand
                RummyServerNamespace.to(matchId).emit("update_player_hand", {
                  playerId,
                  cards: playerHands[matchId][playerId],
                });
              } else {
                console.log("Card not found in player's hand");
                socket.emit("error", {
                  message: "Card not found in player's hand",
                });
              }
            } catch (error) {
              socket.emit("error", {
                message: error.message,
              });
            }
          });

          // =============== EVENT: card_taken_from_discard =================
          socket.on("card_taken_from_discard", (data) => {
            const { cardCode } = data;
            if (!discardPiles[matchId] || discardPiles[matchId].length === 0) {
              socket.emit("error", { message: "No cards in discard pile" });
              return;
            }

            const cardIndex = discardPiles[matchId].findIndex(
              (card) => card.code === cardCode
            );
            if (cardIndex === -1) {
              console.log("Card not found in the discard pile");
              socket.emit("error", {
                message: "Card not found in the discard pile",
              });
              return;
            }

            const card = discardPiles[matchId].splice(cardIndex, 1)[0];
            playerHands[matchId][playerId].push(card);

            // Emit an update to the player's hand
            RummyServerNamespace.to(matchId).emit("update_player_hand", {
              playerId,
              cards: playerHands[matchId][playerId],
            });

            RummyServerNamespace.to(matchId).emit(
              "card_taken_from_discard_confirm",
              {
                playerId,
                cardCode: card.code,
                discardPiles: discardPiles[matchId],
              }
            );
          });

          /**
           * Helper to handle "Before next turn" logic:
           * - Check draw pile size
           * - If <= 1, reshuffle from discard pile (except the top card)
           * - Emit "shuffle_pile"
           * - Wait for "pile_shuffled" from all players
           */

          // =============== EVENT: pass_turn =================
          socket.on("pass_turn", () => {
            console.log(`${playerId} has passed their turn.`);
            RummyServerNamespace.to(matchId).emit("turn_passed", {
              playerId,
            });

            // Move to the next player's turn
            currentPlayerTurn[matchId] =
              (currentPlayerTurn[matchId] + 1) % playerCount;
            console.log(
              "🚀 ~ socket.on ~ currentPlayerTurn:",
              currentPlayerTurn
            );
            handleNextTurn(matchId, currentPlayerTurn[matchId]);
          });

          // =============== EVENT: end_turn =================
          socket.on("end_turn", () => {
            console.log(`${playerId} has ended their turn.`);
            RummyServerNamespace.to(matchId).emit("turn_ended", {
              playerId,
            });

            // Move to the next player's turn
            currentPlayerTurn[matchId] =
              (currentPlayerTurn[matchId] + 1) % playerCount;
            handleNextTurn(matchId, currentPlayerTurn[matchId]);
          });

          // =============== EVENT: pile_shuffled =================
          // All clients must confirm they have updated
          socket.on("pile_shuffled", () => {
            // Mark this player's shuffle confirmation
            shuffleConfirmations[matchId].add(playerId);

            // If everyone has confirmed
            if (shuffleConfirmations[matchId].size === playerCount) {
              console.log("All players have confirmed pile shuffle.");
              // Now proceed with the next turn
              const currentIndex = currentPlayerTurn[matchId];
              const nextPlayerId = game.players[currentIndex];
              RummyServerNamespace.to(matchId).emit("next_turn", {
                playerId: nextPlayerId,
              });
            }
          });
          function handleNextTurn(matchId, currentPlayerIndex) {
            console.log(
              "🚀 ~ handleNextTurn ~ currentPlayerIndex:",
              currentPlayerIndex
            );
            // If there's only 1 card left, reshuffle
            if (drawPiles[matchId]?.length <= 1) {
              // Reset shuffleConfirmations for this round
              shuffleConfirmations[matchId] = new Set();
              console.log(
                "🚀 ~ handleNextTurn ~ shuffleConfirmations:",
                shuffleConfirmations
              );

              // Keep the top card from discard pile
              const topDiscardCard =
                discardPiles[matchId][discardPiles[matchId].length - 1];

              // Take all except top card
              let pileToShuffle = discardPiles[matchId].slice(
                0,
                discardPiles[matchId].length - 1
              );

              // Clear discard pile; put only top card back
              discardPiles[matchId] = [topDiscardCard];

              // Shuffle
              pileToShuffle = shuffleArray(pileToShuffle);
              console.log(
                "🚀 ~ handleNextTurn ~ pileToShuffle:",
                pileToShuffle
              );

              // Put them back into the draw pile
              drawPiles[matchId].push(...pileToShuffle);

              // Emit "shuffle_pile" event to all
              RummyServerNamespace.to(matchId).emit("shuffle_pile", {
                topCard: topDiscardCard,
              });
            } else {
              // If no reshuffle needed, proceed directly
              const nextPlayerId = game.players[currentPlayerIndex];
              console.log("🚀 ~ handleNextTurn ~ nextPlayerId:", nextPlayerId);
              RummyServerNamespace.to(matchId).emit("next_turn", {
                playerId: nextPlayerId,
              });
            }
          }

          const shuffleArray = (array) => {
            for (let i = array.length - 1; i > 0; i--) {
              const j = Math.floor(Math.random() * (i + 1));
              [array[i], array[j]] = [array[j], array[i]];
            }
            return array;
          };
          // =============== EVENT: game_ended =================
          socket.on("game_ended", async (data) => {
            try {
              const { winnerId, loserId, winnerPoints, loserPoints } = data;
              console.log(
                "🚀 ~ socket.on ~ winnerId, loserId, winnerPoints, loserPoints:",
                "matchID",
                winnerId,
                loserId,
                winnerPoints,
                loserPoints,
                matchId
              );

              if (
                !rummyConnectedPlayers[matchId] ||
                rummyConnectedPlayers[matchId].length < 2
              ) {
                console.log("Not enough players to determine winner/loser.");
                return;
              }

              // Handle Points Rummy
              if (gameType === "Points") {
                // Update match results for Points Rummy
                await updateMatch(winnerId, loserId, matchId);
                RummyServerNamespace.to(matchId).emit("on_game_ended", {
                  winner: winnerId,
                  losers: [loserId],
                });

                // Cleanup
                delete rummyConnectedPlayers[matchId];
                delete playerHands[matchId];
                delete drawPiles[matchId];
                delete discardPiles[matchId];
                delete cardCache[matchId];
                return;
              }

              // Handle Deal Rummy
              else if (gameType === "Deals") {
                // Update match wins for the winner
                matchWins[matchId][winnerId] += 1;
                console.log(
                  "🚀 ~ socket.on ~ matchWins[matchId][winnerId]:",
                  matchWins[matchId][winnerId]
                );

                console.log(
                  "🚀 ~ socket.on ~ matchWins[matchId]",
                  matchWins[matchId]
                );
                // Check if the winner has won 2 matches
                if (matchWins[matchId][winnerId] >= 2) {
                  // Declare the winner and end the game
                  RummyServerNamespace.in(matchId).emit("final_winner", {
                    winner: winnerId,
                    matchWins: matchWins[matchId],
                  });

                  // Update match results
                  await updateMatch(winnerId, loserId, matchId);

                  // Cleanup
                  delete rummyConnectedPlayers[matchId];
                  delete playerHands[matchId];
                  delete drawPiles[matchId];
                  delete discardPiles[matchId];
                  delete cardCache[matchId];
                  delete dealTracker[matchId];
                  delete cumulativeScores[matchId];
                  delete matchWins[matchId];
                  return; // Exit early, no need to proceed further
                }

                // If no player has won 2 matches yet, proceed with the next deal
                // Update cumulative scores
                cumulativeScores[matchId][winnerId] += winnerPoints;
                cumulativeScores[matchId][loserId] += loserPoints;

                // Increment the deal counter
                dealTracker[matchId] += 1;

                // Check if the required number of deals has been reached
                const totalDeals = 3; // Set the total number of deals
                if (dealTracker[matchId] < totalDeals) {
                  console.log(
                    "🚀 ~ socket.on ~ dealTracker[matchId]:",
                    dealTracker[matchId]
                  );
                  console.log("🚀 ~ socket.on ~ totalDeals:", totalDeals);

                  // Start a new deal
                  RummyServerNamespace.to(matchId).emit("new_deal", {
                    dealNumber: dealTracker[matchId],
                    cumulativeScores: cumulativeScores[matchId],
                    matchWins: matchWins[matchId],
                  });

                  // Reset the game state for the new deal
                  rummyPlayerReadiness[matchId] = new Set();
                  cardsDealtTracker[matchId] = new Set();
                  shuffleConfirmations[matchId] = new Set();
                  playerHands[matchId] = {};
                  drawPiles[matchId] = [];
                  discardPiles[matchId] = [];
                  cardCache[matchId] = {};

                  // Emit an event to reset the client-side game state
                  setTimeout(() => {
                    console.log("Emitting reset_game_state event");
                    RummyServerNamespace.to(matchId).emit("reset_game_state");
                  }, 5000);
                  return;
                } else {
                  // Determine the final winner based on cumulative scores
                  let finalWinnerId = null;
                  let lowestScore = Infinity;

                  for (const playerId in cumulativeScores[matchId]) {
                    if (cumulativeScores[matchId][playerId] < lowestScore) {
                      lowestScore = cumulativeScores[matchId][playerId];
                      finalWinnerId = playerId;
                    }
                  }
                  console.log("🚀 ~ socket.on ~ finalWinnerId:", finalWinnerId);
                  console.log("🚀 ~ socket.on ~ lowestScore:", lowestScore);

                  // Emit the final winner
                  RummyServerNamespace.to(matchId).emit("final_winner", {
                    winner: finalWinnerId,
                    cumulativeScores: cumulativeScores[matchId],
                    matchWins: matchWins[matchId],
                  });

                  // Update match results
                  await updateMatch(finalWinnerId, loserId, matchId);

                  // Cleanup
                  delete rummyConnectedPlayers[matchId];
                  delete playerHands[matchId];
                  delete drawPiles[matchId];
                  delete discardPiles[matchId];
                  delete cardCache[matchId];
                  delete dealTracker[matchId];
                  delete cumulativeScores[matchId];
                  delete matchWins[matchId];
                  return;
                }
              }

              // Handle Pool Rummy (Pool-101 or Pool-201)
              else if (gameType === "101 Pool" || gameType === "201 Pool") {
                // Update scores for the winner and loser
                poolScores[matchId][winnerId] += +winnerPoints;
                poolScores[matchId][loserId] += +loserPoints;

                // Check if any player has reached the pool limit (101 or 201)
                const poolLimit = gameType === "101 Pool" ? 101 : 201;
                let gameEnded = false;
                let finalWinnerId = null;

                console.log(
                  "🚀 ~ socket.on ~ poolScores[matchId][playerId]:",
                  poolScores[matchId][playerId]
                );
                for (const playerId in poolScores[matchId]) {
                  if (poolScores[matchId][playerId] >= poolLimit) {
                    gameEnded = true;
                    finalWinnerId = playerId;
                    break;
                  }
                }
                console.log("🚀 ~ socket.on ~ gameEnded:", gameEnded);

                console.log("🚀 ~ socket.on ~ finalWinnerId:", finalWinnerId);

                // If the game has ended, declare the final winner
                if (gameEnded) {
                  RummyServerNamespace.to(matchId).emit("final_winner", {
                    winner: finalWinnerId,
                    matchWins: poolScores[matchId],
                  });

                  // Update match results
                  await updateMatch(finalWinnerId, loserId, matchId);

                  // Cleanup
                  delete rummyConnectedPlayers[matchId];
                  delete playerHands[matchId];
                  delete drawPiles[matchId];
                  delete discardPiles[matchId];
                  delete cardCache[matchId];
                  delete poolScores[matchId];
                  return;
                } else {
                  // Start a new round
                  RummyServerNamespace.to(matchId).emit("new_round", {
                    poolScores: poolScores[matchId],
                  });
                  console.log(
                    "🚀 ~ RummyServerNamespace.to ~ poolScores:",
                    poolScores
                  );

                  // Reset the game state for the new round
                  rummyPlayerReadiness[matchId] = new Set();
                  cardsDealtTracker[matchId] = new Set();
                  shuffleConfirmations[matchId] = new Set();
                  playerHands[matchId] = {};
                  drawPiles[matchId] = [];
                  discardPiles[matchId] = [];
                  cardCache[matchId] = {};

                  // Emit an event to reset the client-side game state
                  setTimeout(() => {
                    console.log("Emitting reset_game_state event");
                    RummyServerNamespace.to(matchId).emit("reset_game_state");
                  }, 5000);
                  return;
                }
              }
            } catch (error) {
              console.error("Error in game_ended event:", error);
            }
          });

          socket.on("disconnect", async () => {
            if (rummyConnectedPlayers[matchId]) {
              rummyConnectedPlayers[matchId].forEach(async (id) => {
                await removePlayerFromDB(id);
              });
            }
            console.log("Disconnected Socket : " + socket.id);

            try {
              const index = rummyConnectedPlayers[matchId]?.findIndex(
                (id) => id === playerId
              );
              if (index !== -1) {
                rummyConnectedPlayers[matchId]?.splice(index, 1);
              } else {
                console.log(
                  "Player id not found in Rummy Connected List: " + playerId
                );
              }

              removePlayerFromList(onlinePlayers, playerId);
              RummyServerNamespace.to(matchId).emit("player_disconnected", {
                playerId,
              });

              if (rummyConnectedPlayers[matchId]?.length === 1) {
                console.log("Only one player left. Ending the game.");
                const winnerId = rummyConnectedPlayers[matchId][0];
                const loserId = playerId;

                await updateMatch(winnerId, loserId, matchId);
                RummyServerNamespace.to(matchId).emit("end_game", {
                  message: "Game ended due to only one player remaining.",
                  winner: winnerId,
                });

                // Cleanup
                delete rummyConnectedPlayers[matchId];
                delete playerHands[matchId];
                delete drawPiles[matchId];
                delete discardPiles[matchId];
                delete cardCache[matchId];
              }
            } catch (error) {
              console.error("Error during disconnect:", error);
              removePlayerFromList(onlinePlayers, playerId);
            }
          });

          // =============== ENHANCED GAME EVENT HANDLERS =================

          // Handle player dropped from Pool Rummy
          socket.on("player_dropped", async (playerDroppedData) => {
            try {
              console.log(`🏃 [Backend] Player dropped:`, playerDroppedData);
              
              // Update pool scores with penalty
              if (poolScores[matchId] && poolScores[matchId][playerDroppedData.playerId] !== undefined) {
                poolScores[matchId][playerDroppedData.playerId] += playerDroppedData.penaltyPoints;
                console.log(`💰 [Backend] Applied penalty ${playerDroppedData.penaltyPoints} to player ${playerDroppedData.playerId}`);
              }
              
              // Broadcast player dropped event to all clients
              RummyServerNamespace.to(matchId).emit("player_dropped", {
                ...playerDroppedData,
                timestamp: new Date(),
                poolScores: poolScores[matchId]
              });
              
              console.log(`✅ [Backend] Player drop event broadcasted for match ${matchId}`);
            } catch (error) {
              console.error("❌ [Backend] Error handling player_dropped:", error);
            }
          });

          // Handle player eliminated from Pool Rummy
          socket.on("player_eliminated", async (playerEliminatedData) => {
            try {
              console.log(`❌ [Backend] Player eliminated:`, playerEliminatedData);
              
              // Mark player as eliminated
              if (!eliminatedPlayers[matchId]) {
                eliminatedPlayers[matchId] = [];
              }
              if (!eliminatedPlayers[matchId].includes(playerEliminatedData.playerId)) {
                eliminatedPlayers[matchId].push(playerEliminatedData.playerId);
              }
              
              // Broadcast elimination event
              RummyServerNamespace.to(matchId).emit("player_eliminated", {
                ...playerEliminatedData,
                timestamp: new Date(),
                eliminatedPlayers: eliminatedPlayers[matchId],
                remainingPlayers: game.players.filter(p => !eliminatedPlayers[matchId].includes(p))
              });
              
              console.log(`✅ [Backend] Player elimination broadcasted for match ${matchId}`);
            } catch (error) {
              console.error("❌ [Backend] Error handling player_eliminated:", error);
            }
          });

          // Handle deal completed in Deals Rummy
          socket.on("deal_completed", async (dealCompletedData) => {
            try {
              console.log(`🎯 [Backend] Deal completed:`, dealCompletedData);
              
              // Update deal tracker
              if (dealTracker[matchId] !== undefined) {
                dealTracker[matchId]++;
              }
              
              // Update cumulative scores and deals won
              if (cumulativeScores[matchId] && dealCompletedData.cumulativeScores) {
                Object.assign(cumulativeScores[matchId], dealCompletedData.cumulativeScores);
              }
              
              if (matchWins[matchId] && dealCompletedData.dealsWon) {
                Object.assign(matchWins[matchId], dealCompletedData.dealsWon);
              }
              
              // Broadcast deal completion
              RummyServerNamespace.to(matchId).emit("deal_completed", {
                ...dealCompletedData,
                timestamp: new Date(),
                totalDealsCompleted: dealTracker[matchId]
              });
              
              console.log(`✅ [Backend] Deal completion broadcasted for match ${matchId}`);
            } catch (error) {
              console.error("❌ [Backend] Error handling deal_completed:", error);
            }
          });

          // Handle new deal started
          socket.on("deal_started", async (dealStartedData) => {
            try {
              console.log(`🎲 [Backend] New deal started:`, dealStartedData);
              
              // Reset game state for new deal
              rummyPlayerReadiness[matchId] = new Set();
              cardsDealtTracker[matchId] = new Set();
              
              // Broadcast deal start
              RummyServerNamespace.to(matchId).emit("deal_started", {
                ...dealStartedData,
                timestamp: new Date(),
                gameState: "new_deal"
              });
              
              console.log(`✅ [Backend] Deal start broadcasted for match ${matchId}`);
            } catch (error) {
              console.error("❌ [Backend] Error handling deal_started:", error);
            }
          });

          // Handle Pool game ended
          socket.on("pool_game_ended", async (poolGameEndedData) => {
            try {
              console.log(`🏆 [Backend] Pool game ended:`, poolGameEndedData);
              
              // Broadcast pool game end
              RummyServerNamespace.to(matchId).emit("pool_game_ended", {
                ...poolGameEndedData,
                timestamp: new Date(),
                finalPoolScores: poolScores[matchId]
              });
              
              // Cleanup pool data
              delete poolScores[matchId];
              delete eliminatedPlayers[matchId];
              
              console.log(`✅ [Backend] Pool game end broadcasted for match ${matchId}`);
            } catch (error) {
              console.error("❌ [Backend] Error handling pool_game_ended:", error);
            }
          });

          // Handle cumulative score updates
          socket.on("cumulative_score_updated", async (cumulativeScoreData) => {
            try {
              console.log(`📊 [Backend] Cumulative score updated:`, cumulativeScoreData);
              
              // Update cumulative scores
              if (!cumulativeScores[matchId]) {
                cumulativeScores[matchId] = {};
              }
              cumulativeScores[matchId][cumulativeScoreData.playerId] = cumulativeScoreData.cumulativeScore;
              
              // Broadcast score update
              RummyServerNamespace.to(matchId).emit("cumulative_score_updated", {
                ...cumulativeScoreData,
                timestamp: new Date(),
                allScores: cumulativeScores[matchId]
              });
              
              console.log(`✅ [Backend] Score update broadcasted for match ${matchId}`);
            } catch (error) {
              console.error("❌ [Backend] Error handling cumulative_score_updated:", error);
            }
          });

          // Handle active players update
          socket.on("active_players_updated", async (activePlayersData) => {
            try {
              console.log(`👥 [Backend] Active players updated:`, activePlayersData);
              
              // Broadcast active players update
              RummyServerNamespace.to(matchId).emit("active_players_updated", {
                ...activePlayersData,
                timestamp: new Date(),
                matchId: matchId
              });
              
              console.log(`✅ [Backend] Active players update broadcasted for match ${matchId}`);
            } catch (error) {
              console.error("❌ [Backend] Error handling active_players_updated:", error);
            }
          });
        } catch (error) {
          console.error(error);
          RummyServerNamespace.in(socket.id).disconnectSockets();
          removePlayerFromList(onlinePlayers, playerId);
        }
      } else {
        console.error("Missing MatchID or playerID");
        RummyServerNamespace.in(socket.id).disconnectSockets();
      }
    });

    GameServerNamespace.on("connection", async (socket) => {
      const { matchId, playerId } = socket.handshake.query;
      let playerCount;
      let game;

      if (matchId && playerId) {
        try {
          game = await Game.findById(matchId);
          if (!game) throw new Error("Game not found");
          if (!game.players.includes(playerId))
            throw new Error("Player not authorized for this game");
          // if (game.gameWonDate !== null) throw new Error('Game already over');

          playerCount = game.players.length;

          socket.join(game.id);

          const newPlayer = {
            socketId: socket.id,
            playerId,
            contestId: game.tableId,
          };
          console.log(playerId, "Connected");
          removeDuplicatePlayer(playerId, io);

          console.log("Player added at gameserver");
          onlinePlayers.push(newPlayer);

          await addPlayerToDB(newPlayer);

          // Initialize readiness tracking for this match
          if (!playerReadiness[matchId]) {
            playerReadiness[matchId] = new Set();
          }
          if (!connectedPlayer[matchId]) {
            connectedPlayer[matchId] = [];
          }

          //Removed Start MAtch Logic from here
          //   const matchedPlayers = onlinePlayers.filter((player) =>
          //     game.players.includes(player.playerId)
          //   );

          //   if (matchedPlayers.length === game.players.length) {
          //     console.log("Online Match Players : " + matchedPlayers);
          //     GameServerNamespace.to(game.id).emit("start_match", {
          //       playerId: game.players[0],
          //     });
          //   }
        } catch (error) {
          console.error(error);
          GameServerNamespace.in(socket.id).disconnectSockets();
        }
      } else {
        console.error("Missing MatchID or playerID");
        GameServerNamespace.in(socket.id).disconnectSockets();
      }

      //When Ready Event Pushed
      socket.on("player_ready", () => {
        if (!playerCount) {
          console.error("Player count is not defined");
          return;
        }
        // Mark this player as ready
        playerReadiness[matchId].add(playerId);

        connectedPlayer[matchId].push(playerId);

        // Check if all players are ready
        if (playerReadiness[matchId].size === playerCount) {
          console.log("All players are ready. Starting the match.");
          GameServerNamespace.to(matchId).emit("start_match", {
            playerId: game.players[0],
          });

          // Optionally, clear readiness tracking after the match starts
          delete playerReadiness[matchId];
        }
      });

      // Handle dice click event
      socket.on("Dice_Clicked", async (data) => {
        GameServerNamespace.to(matchId).emit("Dice_Clicked", data);
      });

      // Handle dice rolled event
      socket.on("Dice_Rolled", async (data) => {
        GameServerNamespace.to(matchId).emit("Dice_Rolled", data);
      });

      // Handle move token event
      socket.on("MoveToken", async (data) => {
        GameServerNamespace.to(matchId).emit("MoveToken", data);
      });

      // Handle game finished event
      socket.on("Game_Finished", async (data) => {
        try {
          console.log("Game Finished : ", data);
          const { reason, playerId } = data;

          if (!reason)
            throw new Error("Reason Not Found in Game Finished Event");

          let winnerId;
          let loserId;
          let winnersArray;

          if (reason === "won") {
            console.log("Game Finished : Reason Won");
            winnerId = playerId;
            winnersArray = await updateMatch(winnerId, "", matchId);
          } else if (reason === "misstry") {
            console.log("Game Finished : Reason Misstry");
            loserId = playerId;
            winnersArray = await updateMatch("", loserId, matchId);
          } else if (reason === "time_runout") {
            console.log("Game Finished : Timer Runout");
            winnerId = playerId;
            winnersArray = await updateMatch(winnerId, "", matchId);
          } else {
            console.log("Game Finished : ", reason);
            throw new Error("Invalid reason provided in ", reason);
          }

          GameServerNamespace.to(matchId).emit("Game_Finished", data);

          if (winnersArray.length > 0) {
            console.log("Winners Array : " + winnersArray);
          } else {
            throw new Error("Invalid Winners Array : " + winnersArray);
          }

          winnersArray.forEach(async (pId) => {
            disconnectPlayer(pId, io);
            removePlayerFromList(onlinePlayers, pId);
            removePlayerFromList(playersSearching, pId);
            await removePlayerFromDB(pId);
          });
        } catch (error) {
          console.error(
            "An error occurred while processing Game_Finished event : ",
            error
          );
        }
      });

      // Handle turn missed event
      socket.on("Turn_Missed", async (data) => {
        GameServerNamespace.to(matchId).emit("Turn_Missed", data);
      });

      // Handle turn finished event
      socket.on("Turn_Finished", async (data) => {
        GameServerNamespace.to(matchId).emit("NextTurn", data);
      });

      // Handle Emoji
      socket.on("emoji_sent", async (data) => {
        GameServerNamespace.to(matchId).emit("Emoji_Reaction", data);
      });

      // Handle turn finished event
      socket.on("Turn_Finished2", async (data) => {
        if (!turnAcknowledgements[matchId]) {
          turnAcknowledgements[matchId] = new Set();
        }

        // Add the player's acknowledgement
        turnAcknowledgements[matchId].add(playerId);

        if (
          turnAcknowledgements[matchId].size === connectedPlayer[matchId].length
        ) {
          // All players have acknowledged, emit the next turn event
          GameServerNamespace.to(matchId).emit("NextTurn", data);

          // Clear the acknowledgements set for the next turn
          turnAcknowledgements[matchId].clear();
        }
      });

      // Handle match cancelled event
      socket.on("Match_Cancelled", async (data) => {
        try {
          let winnerId = playerId;
          let winnersArray = await updateMatch(winnerId, "", matchId);

          if (winnersArray.length > 0) {
            console.log("Winners Array : " + winnersArray);
          } else {
            throw new Error("Invalid Winners Array in Match : " + winnersArray);
          }

          winnersArray.forEach(async (pId) => {
            disconnectPlayer(pId, io);
            removePlayerFromList(onlinePlayers, pId);
            removePlayerFromList(playersSearching, pId);
            await removePlayerFromDB(pId);
          });
        } catch (error) {
          console.error(
            "An error occurred during match cancelled event:",
            error
          );
        }

        logOnlinePlayers();
      });

      socket.on("disconnect", async () => {
        console.log("Disconnected Socket : " + socket.id);

        // if (playerReadiness[matchId]) {
        //   delete playerReadiness[matchId];
        // }

        try {
          //Remove From Connected Players
          const index = connectedPlayer[matchId].findIndex((id) => {
            return id === playerId;
          });
          if (index !== -1) {
            connectedPlayer[matchId].splice(index, 1);
          } else {
            console.log(
              "Player id not found in the ConnectedList : " + playerId
            );
          }

          // Notify the room about the disconnection
          GameServerNamespace.to(matchId).emit("player_disconnected", {
            playerId,
          });

          // Handle 2-player match (end game if 1 player disconnects)
          if (game.players.length === 2) {
            console.log(
              "2-player match: Ending game as one player disconnected."
            );

            let loserId = playerId;

            let winnersArray = await updateMatch("", loserId, matchId);

            if (winnersArray.length > 0) {
              console.log("Winners Array : " + winnersArray);
            } else {
              throw new Error(
                "Invalid Winners Array in Match : " + winnersArray
              );
            }

            // Perform post-game cleanup
            winnersArray.forEach(async (pId) => {
              console.log("Player id to remove: " + pId);
              removePlayerFromList(onlinePlayers, pId.toString());
              removePlayerFromList(playersSearching, pId.toString());
              await removePlayerFromDB(pId.toString());
              disconnectPlayer(pId.toString(), io);
            });
          }
          // Handle 4-player match (end game only if all players disconnect)
          else if (game.players.length === 4) {
            if (connectedPlayer[matchId].length === 1) {
              console.log(
                "4-player match: All players have disconnected. Ending game."
              );
              let winnersArray = await updateMatch(
                connectedPlayer[matchId][0],
                "",
                matchId
              );

              winnersArray.forEach(async (pId) => {
                console.log("Player id to remove: " + pId);
                removePlayerFromList(onlinePlayers, pId.toString());
                removePlayerFromList(playersSearching, pId.toString());
                await removePlayerFromDB(pId.toString());
                disconnectPlayer(pId.toString(), io);
              });
            } else {
              console.log(
                `4-player match: ${connectedPlayer[matchId].length} players remaining. Continuing game.`
              );

              // Remove disconnected player from lists
              removePlayerFromList(onlinePlayers, playerId.toString());
              removePlayerFromList(playersSearching, playerId.toString());
              await removePlayerFromDB(playerId.toString());
              disconnectPlayer(playerId.toString(), io);
            }
          }
        } catch (error) {
          console.error("An error occurred during disconnect event:", error);
          removePlayerFromList(onlinePlayers, playerId.toString());
          removePlayerFromList(playersSearching, playerId.toString());
          await removePlayerFromDB(playerId.toString());
          disconnectPlayer(playerId.toString(), io);
        }
        logOnlinePlayers();
      });
    });

    io.on("connection", async (socket) => {
      const { playerId, contestId } = socket.handshake.query;

      if (playerId && contestId) {
        const player = {
          socketId: socket.id,
          playerId,
          contestId,
        };

        removeDuplicatePlayer(playerId, io);
        console.log("Player added at match making");
        onlinePlayers.push(player);

        await addPlayerToDB(player);
        playersSearching.push(player);

        let table = await Table.findById(contestId);
        console.log(table, "Players count");
        let count;
        if (table.gameType === "2 Player") {
          count = 2;
        } else if (table.gameType === "4 Player") {
          count = 4;
        }
        if (count > 0) {
          await matchPlayers(player, count, io);
        } else {
          console.log("Invalid Player Count", count);
        }

        logOnlinePlayers();
      } else {
        console.error("Player ID and contest ID are required");
        socket.disconnect();
      }

      socket.on("disconnect", async (reason) => {
        console.log("Disconnected Socket : " + socket.id);
        console.log("Reason : " + reason);
        removePlayerFromList(onlinePlayers, playerId);
        removePlayerFromList(playersSearching, playerId);
        await removePlayerFromDB(playerId);
        logOnlinePlayers();
      });
    });
  },
};

/**
 * Below are helper functions and references to existing logic
 * (unchanged, but included for clarity)
 */

// Helper to log connected players
const logOnlinePlayers = () => {
  // console.log("Online players:", onlinePlayers);
  console.log("Total online players:", onlinePlayers.length);
  console.log("Players Searching:", playersSearching.length);
};

function disconnectPlayer(socketId, io) {
  const targetSocket = io.sockets.sockets.get(socketId);
  if (targetSocket) {
    targetSocket.disconnect(true);
    console.log("Target Socket Disconnected");
  } else {
    console.log("Socket is not connected", socketId);
  }
}

function removePlayerFromList(list, playerId) {
  const index = list.findIndex((player) => {
    // console.log("Player ID type ", typeof player.playerId);
    // console.log("Player ID type ", typeof playerId);
    return player.playerId === playerId;
  });
  if (index !== -1) {
    list?.splice(index, 1);
  } else {
    console.log("Player id not found in the list:", playerId);
  }
}

function removeDuplicatePlayer(playerId, io) {
  let existingPlayer;
  [onlinePlayers, playersSearching].forEach(
    (list) =>
      (existingPlayer = list.find((player) => player.playerId === playerId)) &&
      disconnectPlayer(existingPlayer.socketId, io) &&
      removePlayerFromList(list, existingPlayer.playerId)
  );
}

const matchPlayers = async (newPlayer, numberOfPlayersToMatch, io) => {
  const matchedPlayers = [];
  matchedPlayers.push(newPlayer);

  // Find players with the same contestId
  const sameContestPlayer = playersSearching.filter(
    (player) =>
      player.playerId !== newPlayer.playerId &&
      player.contestId === newPlayer.contestId
  );

  sameContestPlayer.forEach((p) => matchedPlayers.push(p));

  if (matchedPlayers.length >= numberOfPlayersToMatch) {
    console.log("Matched Players", matchedPlayers);

    try {
      const table = await Table.findById(newPlayer.contestId);
      if (!table) throw new Error("Table not found");

      const game = new Game({
        players: matchedPlayers.map((player) => player.playerId),
        tableId: newPlayer.contestId,
        gameStartedDate: new Date(),
        winner: [],
      });

      await game.save();
      const matchId = game._id.toString();

      // Notify all matched players
      matchedPlayers.forEach((player) => {
        io.to(player.socketId).emit("match_found", { matchId });
      });

      // Disconnect matched players
      matchedPlayers.forEach((player) => {
        disconnectPlayer(player.socketId, io);
      });

      // Remove them from lists
      matchedPlayers.forEach(async (player) => {
        removePlayerFromList(onlinePlayers, player.playerId);
        removePlayerFromList(playersSearching, player.playerId);
        await removePlayerFromDB(player.playerId);
      });

      console.log(
        `Matched ${numberOfPlayersToMatch} players with matchId: ${matchId}`
      );
    } catch (error) {
      console.error("Error creating match:", error);
    }
  }
};

const updateMatch = async (winnerId, loserId, matchId) => {
  try {
    if (matchId) {
      const game = await Game.findById(matchId);
      if (!game) throw new Error("Game not found: " + matchId);
      if (game.winner.length > 0)
        throw new Error("Game is already Finish. No need to update.");

      const endDate = new Date();
      game.gameWonDate = endDate;
      if (winnerId) {
        console.log("Winner ID Found");
        loserId = game.players.find(
          (id) => id.toString() !== winnerId.toString()
        );
        console.log(`winner : ${winnerId} looser : ${loserId}`);
      } else if (loserId) {
        console.log("Looser ID Found");
        winnerId = game.players.find(
          (id) => id.toString() !== loserId.toString()
        );
        console.log(`winner : ${winnerId} looser : ${loserId}`);
      } else {
        throw new Error(
          `Winner : ${winnerId} or Looser : ${loserId} ID not Found`
        );
      }

      const loosers = game.players.filter(
        (id) => id.toString() !== winnerId.toString()
      );
      game.winner = [winnerId, ...loosers];

      await game.save();

      // Clear the acknowledgements set for the next turn
      turnAcknowledgements[matchId]?.clear();
      connectedPlayer[matchId] = [];
      playerReadiness[matchId]?.clear();

      const table = await Table.findById(game.tableId);
      if (!table) throw new Error("Table not found: " + game.tableId);

      const prizeAmount = table.wonCoin;
      const feeAmount = table.bet;
      const commission = table.rake;

      //comission transaction
      const commissionsTransactionData = {
        amount: commission,
        gameName: "Ludo",
        status: "Approved",
        title: "comission",
        description: `comission of ${commission} added`,
        transactionType: "commission",
        transactionInto: "admin",
      };

      await Transaction.create(commissionsTransactionData);

      const winner = await User.findById(winnerId);

      if (!winner) throw new Error(`Winner : ${winner} object not Found`);
      //------------
      for (const loserId of loosers) {
        const loser = await User.findById(loserId);
        if (!loser) {
          console.error(`Loser not found for ID: ${loserId}`);
          continue; // or throw an error if you prefer
        }

        const loserTransactionData = {
          userId: loserId,
          amount: feeAmount,
          status: "Approved",
          gameName: "Ludo",
          title: "Fee",
          description: `Fee deducted of ${feeAmount}`,
          transactionType: "fee",
          transactionInto: "user",
        };
        await Transaction.create(loserTransactionData);

        let remainingFee = feeAmount;
        let totalLoserDeduction = 0;
        const bonusDeduction = remainingFee * 0.1;

        // First: bonus
        if (loser.cashBonus >= bonusDeduction) {
          loser.cashBonus -= bonusDeduction;
          remainingFee -= bonusDeduction;
          totalLoserDeduction += bonusDeduction;
        } else {
          remainingFee -= loser.cashBonus;
          totalLoserDeduction += loser.cashBonus;
          loser.cashBonus = 0;
        }

        // Second: deposit
        if (loser.depositBalance >= remainingFee) {
          loser.depositBalance -= remainingFee;
          totalLoserDeduction += remainingFee;
          remainingFee = 0;
        } else {
          remainingFee -= loser.depositBalance;
          totalLoserDeduction += loser.depositBalance;
          loser.depositBalance = 0;
        }

        // Third: winning
        if (loser.winningBalance >= remainingFee) {
          loser.winningBalance -= remainingFee;
          totalLoserDeduction += remainingFee;
          remainingFee = 0;
        }

        // If still > 0, not enough balance
        if (remainingFee > 0) {
          console.error(
            `Loser ${loserId} does not have enough balance to cover the fee.`
          );
          // Decide how to handle this: throw error, partial update, etc.
        }

        console.log(
          "Total Loser Deduction for",
          loserId,
          ":",
          totalLoserDeduction
        );

        // 5. Finally, save the updated loser
        await loser.save();
      }
      //------------

      // Calculate the net prize amount after deducting the fee
      const netPrizeAmount = prizeAmount - feeAmount;

      // // Deduct the fee from the winner's balances in the same manner (bonus > deposit > winning)
      // let remainingWinnerFee = feeAmount;

      // // Deduct 10% of the fee from the bonus balance, if enough
      // const winnerBonusDeduction = remainingWinnerFee * 0.1;
      // if (winner.bonusBalance >= winnerBonusDeduction) {
      //   winner.bonusBalance -= winnerBonusDeduction;
      //   remainingWinnerFee -= winnerBonusDeduction;
      // } else {
      //   remainingWinnerFee -= winner.bonusBalance; // Deduct whatever is in the bonus
      //   winner.bonusBalance = 0;
      // }

      // // Deduct the remaining fee from deposit balance, if enough
      // if (winner.depositBalance >= remainingWinnerFee) {
      //   winner.depositBalance -= remainingWinnerFee;
      //   remainingWinnerFee = 0;
      // } else {
      //   remainingWinnerFee -= winner.depositBalance; // Deduct whatever is in the deposit balance
      //   winner.depositBalance = 0;
      // }

      // // Deduct the remaining fee from the winning balance
      // if (winner.winningBalance >= remainingWinnerFee) {
      //   winner.winningBalance -= remainingWinnerFee;
      //   remainingWinnerFee = 0;
      // }

      // // If there is still remaining fee, log an error (shouldn't happen with proper checks)
      // if (remainingWinnerFee > 0) {
      //   console.error("Winner does not have enough balance to cover the fee.");
      // }

      // Add the net prize amount to the winner's winning balance
      winner.winningBalance += netPrizeAmount;

      console.log("Total Winner Winning : ", netPrizeAmount);

      // Create transaction for winner's fee
      const winnerFeeTransactionData = {
        userId: winnerId,
        amount: feeAmount,
        status: "Approved",
        gameName: "Ludo",
        title: "Fee",
        description: `Fee deducted of ${feeAmount}`,
        transactionType: "fee",
        transactionInto: "user",
      };
      await Transaction.create(winnerFeeTransactionData);

      // Create transaction for winner's prize
      const winnerPrizeTransactionData = {
        userId: winnerId,
        amount: prizeAmount,
        status: "Approved",
        gameName: "Ludo",
        title: "Prize",
        description: `Prize won of ${prizeAmount}`,
        transactionType: "prize",
        transactionInto: "user",
      };
      await Transaction.create(winnerPrizeTransactionData);

      await winner.save();

      return game.winner;
    }
  } catch (error) {
    console.error("An error occurred during update match :", error);
    return [];
  }
};

const addPlayerToDB = async (player) => {
  try {
    await OnlinePlayers.updateOne(
      { playerId: player.playerId },
      { contestId: player.contestId },
      { upsert: true }
    );
    console.log("Player added/updated in DB");
  } catch (error) {
    console.error("Error adding player:", error);
  }
};

const removePlayerFromDB = async (playerId) => {
  try {
    await OnlinePlayers.deleteOne({ playerId });
    console.log("Player removed from DB");
  } catch (error) {
    console.error("Error removing player:", error);
  }
};

function calculateDeadwoodPoints(hand) {
  let points = 0;
  for (const card of hand) {
    if (!isPartOfMeld(card, hand)) {
      points += getCardValue(card);
    }
  }
  return points;
}

// Function to determine if a card is part of a valid meld
function isPartOfMeld(card, hand) {
  return isPartOfSet(card, hand) || isPartOfRun(card, hand);
}
function isPartOfSet(card, hand) {
  const sameRankCards = hand.filter(
    (c) => c.value === card.value && c.suit !== card.suit
  );
  return sameRankCards.length >= 2; // At least two other cards of the same rank
}
function isPartOfRun(card, hand) {
  const cardOrder = [
    "ACE",
    "2",
    "3",
    "4",
    "5",
    "6",
    "7",
    "8",
    "9",
    "10",
    "JACK",
    "QUEEN",
    "KING",
  ];
  const cardIndex = cardOrder.indexOf(card.value);
  if (cardIndex === -1) return false;

  const previousCard1 = cardOrder[cardIndex - 1];
  const previousCard2 = cardOrder[cardIndex - 2];
  const nextCard1 = cardOrder[cardIndex + 1];
  const nextCard2 = cardOrder[cardIndex + 2];

  const hasPrevious1 = hand.some(
    (c) => c.value === previousCard1 && c.suit === card.suit
  );
  const hasPrevious2 = hand.some(
    (c) => c.value === previousCard2 && c.suit === card.suit
  );
  const hasNext1 = hand.some(
    (c) => c.value === nextCard1 && c.suit === card.suit
  );
  const hasNext2 = hand.some(
    (c) => c.value === nextCard2 && c.suit === card.suit
  );

  // Check for a run of at least three cards
  return (
    (hasPrevious1 && hasPrevious2) ||
    (hasPrevious1 && hasNext1) ||
    (hasNext1 && hasNext2)
  );
}

// Function to get the value of a card
function getCardValue(card) {
  const value = card.value;
  if (["JACK", "QUEEN", "KING"].includes(value)) {
    return 10;
  } else if (value === "ACE") {
    return 1;
  } else {
    return parseInt(value, 10);
  }
}
