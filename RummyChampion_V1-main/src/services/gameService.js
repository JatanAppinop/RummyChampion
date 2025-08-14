const gameRepository = require('../repositories/gameRepository');
const tableRepository = require('../repositories/tableRepository');
const userRepository = require('../repositories/userRepository')

class gameService {

    async getAllGames(query) {
        return await gameRepository.getAllGames(query);
    }

    async getSettings() {
        return await gameRepository.getSettings();
    }

    async updateSettings(data) {
        return await gameRepository.updateSettings(data);
    }

    async getUserGames(userId) {
        return await gameRepository.getUserGames(userId);
    }

    async getGameDetail(gameId) {
        return await gameRepository.getGameDetails(gameId);
    }

    async updateGameWinner(gameId, winnerId, wonCoin) {
        const game = await gameRepository.updateWinner(gameId, winnerId, wonCoin);
        if (game) {
            await userRepository.updateBalance(winnerId, wonCoin);
        }
        return game;
    }

    async getGameById(gameId) {
        return await gameRepository.findById(gameId);
    }

    async getGamesByType(gameType) {
        return await gameRepository.findByType(gameType);
    }

    async startGame(players, tableId) {
        const table = await tableRepository.findTable(tableId);
        const game = await gameRepository.createGame({
            players,
            tableId,
            gameMode: table.gameMode,
            gameType: table.gameType,
            wonCoin: table.wonCoin,
        });
        return game;
    };

    async endGame(gameId, winner, wonCoin) {
        const game = await gameRepository.updateGame(gameId, {
            winner,
            wonCoin,
            gameWonDate: new Date(),
        });
        return game;
    };

    async joinGame(gameId, userId) {
        const game = await gameRepository.addPlayerToGame(gameId, userId);
        return game;
    };

    async getGameDetails(gameId) {
        const game = await gameRepository.findGameById(gameId);
        return game;
    };

    async getUserGames(userId) {
        const games = await gameRepository.findGamesByUserId(userId);
        return games;
    };

    async getActiveGames() {
        const games = await gameRepository.findActiveGames();
        return games;
    };

    async getLeaderBoard() {
        const leaderBoard = await gameRepository.calculateLeaderBoard();
        return leaderBoard;
    };

    async getAllGameData() {
        return await gameRepository.allGameData();
    };

}

module.exports = new gameService()