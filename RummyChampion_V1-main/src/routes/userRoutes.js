const express = require("express");
const router = express.Router();
const { signup_login, sendOTP, editProfile, profile, verify,
    getUserGames, getShopPlans, purchaseShopPlan, getGameDetails,
    userProfiles, getUserProfile, onlinePlayers
} = require("../controllers/userController");
const { Auth } = require("../middlewares/Auth")

router.post("/user/signUp", signup_login);

router.get('/onlinePlayers', onlinePlayers)

router.post("/user/sendOtp", sendOTP);

router.get('/user/profile', [Auth], profile);

router.get('/user/playerProfile', [Auth], getUserProfile);

router.get('/user/userProfiles/:userId', [Auth], userProfiles)

router.put('/user/editProfile', [Auth], editProfile);

router.get("/user/verifyToken", [Auth], verify);

router.get('/user/games', [Auth], getUserGames);

router.get('/user/game/:gameId', [Auth], getGameDetails);

router.get('/user/shopPlans', [Auth], getShopPlans);

router.post('/user/purchaseShopPlan', [Auth], purchaseShopPlan);

module.exports = router;
