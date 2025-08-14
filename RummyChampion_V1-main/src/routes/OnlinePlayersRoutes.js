const express = require('express');
const { getAllOnlinePlayers,
    getOnlinePlayersByContestId
} = require('../controllers/OnlinePlayersController');
const router = express.Router();
const { Auth } = require("../middlewares/Auth")

router.get('/OnlinePlayers/getAllPlayers', [Auth], getAllOnlinePlayers)

router.get('/OnlinePlayers/playersByContest', [Auth], getOnlinePlayersByContestId)

module.exports = router;