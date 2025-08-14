const express = require('express');
const GameController = require('../controllers/gameController');
const router = express.Router();
const { Auth } = require("../middlewares/Auth")

router.post('/admin/addGame', [Auth], GameController.createGame);

router.get('/admin/settings', [Auth], GameController.getSettings)

router.put('/admin/updateSettings', [Auth], GameController.updateSettings)

router.get('/admin/getGames', [Auth], GameController.getAllGames);

router.put('/admin/updateGameWinner', [Auth], GameController.updateGameWinner);

router.get('/user/getGame/:gameId', [Auth], GameController.getGameById);

router.get('/admin/getGamesByType/:gameType', [Auth], GameController.getGamesByType);

router.post('/game/start', [Auth], GameController.startGame);

router.post('/game/end', [Auth], GameController.endGame);

router.post('/game/join', [Auth], GameController.joinGame);

router.get('/game', [Auth], GameController.getAllGameDetails)

router.get('/game/:gameId', [Auth], GameController.getGameDetails);

router.get('/user/games/:userId', [Auth], GameController.getUserGames);

router.get('/games/active', [Auth], GameController.getActiveGames);

router.get('/user/leaderboard', [Auth], GameController.getLeaderBoard);


/**
 *     
    public static string getContests = "game/getContest";
    public static string getContest = "contest/getContest/";
    public static string getUser = "user/getUser/";
    public static string getMatch = "contest/getmatch/";
    public static string updateMatch = "contest/editMatch/";
 */

module.exports = router;
