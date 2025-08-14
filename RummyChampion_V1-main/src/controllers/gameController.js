const gameService = require('../services/gameService');
const tableService = require('../services/tableService');

const GameController = {
    async createGame(req, res) {
        try {
            const gameData = req.body;
            const newGame = await gameService.createGame(gameData);
            res.status(201).json({ success: true, data: newGame });
        } catch (error) {
            res.status(400).json({ success: false, message: error.message });
        }
    },

    async getSettings(req, res) {
        try {
            const settings = await gameService.getSettings();
            res.json({ success: true, settings });
        } catch (error) {
            console.error(error);
            res.status(500).json({ success: false, message: 'Error retrieving settings' });
        }
    },

    async updateSettings(req, res) {
        try {
            const updatedSettings = await gameService.updateSettings(req.body);
            res.json({ success: true, settings: updatedSettings });
        } catch (error) {
            console.error(error);
            res.status(500).json({ success: false, message: 'Error updating settings' });
        }
    },

    async getAllGames(req, res) {
        try {
            const games = await gameService.getAllGames(req.query);
            res.status(200).json({ success: true, data: games });
        } catch (error) {
            res.status(400).json({ success: false, message: error.message });
        }
    },

    async updateGameWinner(req, res) {
        try {
            const { gameId, winnerId, wonCoin } = req.body;
            const game = await gameService.updateGameWinner(gameId, winnerId, wonCoin);
            res.status(200).json({ success: true, message: 'Game updated successfully', game });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async getGamesByType(req, res) {
        try {
            const { gameType } = req.params;
            const games = await gameService.getGamesByType(gameType);
            res.status(200).json({ success: true, games });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async getGameById(req, res) {
        try {
            const { gameId } = req.params;
            const game = await gameService.getGameById(gameId);
            res.status(200).json({ success: true, data: game });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async startGame(req, res) {
        try {
            const { players, tableId } = req.body;
            const table = await tableService.findTable(tableId);
            if (!table || !table.isActive) {
                return res.status(400).json({ success: false, message: 'Invalid or inactive table' });
            }
            const game = await gameService.startGame(players, tableId);
            res.status(201).json({ success: true, message: 'Game started successfully', data: game });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async endGame(req, res) {
        try {
            const { gameId, winner, wonCoin } = req.body;
            const game = await gameService.endGame(gameId, winner, wonCoin);
            res.status(200).json({ success: true, message: 'Game ended successfully', data: game });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async joinGame(req, res) {
        try {
            const { gameId } = req.body;
            const { userId } = req.user;
            const game = await gameService.joinGame(gameId, userId);
            res.status(200).json({ success: true, message: 'Joined game successfully', data: game });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async getGameDetails(req, res) {
        try {
            const { gameId } = req.params;
            const game = await gameService.getGameDetail(gameId);
            res.status(200).json({ success: true, message: 'Game details retrieved successfully', data: game });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async getAllGameDetails(req, res) {
        try {
            const data = await gameService.getAllGameData();
            res.status(200).json({ success: true, message: 'Games details retrieved successfully', data });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async getUserGames(req, res) {
        try {
            const { userId } = req.params;
            const games = await gameService.getUserGames(userId);
            res.status(200).json({ success: true, message: 'User games retrieved successfully', data: games });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async getActiveGames(req, res) {
        try {
            const games = await gameService.getActiveGames();
            res.status(200).json({ success: true, message: 'Active games retrieved successfully', data: games });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    async getLeaderBoard(req, res) {
        try {
            const leaderBoard = await gameService.getLeaderBoard();
            res.status(200).json({ success: true, message: 'Leader board retrieved successfully', data: leaderBoard });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },
};

module.exports = GameController;
