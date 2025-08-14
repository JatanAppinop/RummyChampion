const { func } = require("joi");
const userService = require("../services/userService");
const gameService = require("../services/gameService")
const shopPlanService = require("../services/shopPlanService")
const { verifyToken } = require("../utils/Jwt");
// const redis = require('redis')

async function signup_login(req, res) {
    const { mobileNumber, otp, refercode } = req.body;
    try {
        const response = await userService.signup(mobileNumber, otp, refercode);

        if (response.success) {
            res.status(200).json({
                success: true,
                message: "Login Successful",
                data: response.data
            });
        } else {
            res.status(400).json({
                success: false,
                message: response.message,
            });
        }
    } catch (error) {
        console.error("Error in signup_login controller:", error);
        res.status(500).json({
            success: false,
            message: "Internal Server Error",
        });
    }
};

async function onlinePlayers(req, res) {
    try {
        const classicCount = await redisClient.get('onlinePlayers:Classic');
        const timerCount = await redisClient.get('onlinePlayers:Timer');
        const turboCount = await redisClient.get('onlinePlayers:Turbo');

        res.json({
            Classic: classicCount || 0,
            Timer: timerCount || 0,
            Turbo: turboCount || 0
        });
    } catch (error) {
        res.status(500).json({ error: 'Failed to fetch online players count' });
    }
};

async function sendOTP(req, res) {
    const { mobileNumber, resend } = req.body;

    try {
        const result = await userService.sendOTP(mobileNumber, resend);
        return res.status(200).json({ success: true, message: "OTP sent successfully", data: result });
    } catch (error) {
        console.error('Error in sendOtp controller:', error);
        return res.status(500).json({ success: false, message: "Failed to send OTP", data: null });
    }
};

async function profile(req, res) {
    const { userId } = req.user;
    try {
        const result = await userService.getProfile(userId);
        res.status(200).json({ success: true, message: "User data", data: result });
    } catch (error) {
        console.error('Error in fetching profile:', error);
        return res.status(500).json({ success: false, message: "Failed to fetch Profile", data: null });
    }
};

async function getUserProfile(req, res) {
    const { userId } = req.user;

    try {
        const userProfile = await userService.getUserProfile(userId);
        res.status(200).json({ success: true, data: userProfile });
    } catch (error) {
        res.status(error.statusCode || 500).json({ message: error.message });
    }
};

async function userProfiles(req, res) {
    const userId = req.params.userId;
    try {
        const data = await userService.getProfile(userId);
        res.status(200).json({ success: true, data, message: "Users data" });
    } catch (error) {
        console.error('Error in fetching profile:', error);
        return res.status(500).json({ success: false, message: "Failed to fetch Profile", data: null });
    }
};


async function editProfile(req, res) {
    const { userId } = req.user;
    try {
        const userData = req.body;
        const result = await userService.updateProfile(userId, userData);
        res.status(200).json(result);
    } catch (err) {
        console.error('Error in updating profile:', error);
        return res.status(500).json({ success: false, message: "Failed to update Profile", data: null });
    }

};

async function verify(req, res) {
    const { mobileNumber } = req.user;
    try {
        const data = await userService.verify(mobileNumber);
        return res.status(200).json({ success: true, message: "Token verified successfully", data });
    } catch (error) {
        console.error("Error verifying token:", error);
        return res.status(500).json({ success: false, message: "Internal server error", data: null });
    }
};

async function getUserGames(req, res) {
    try {
        const { userId } = req.user;
        const games = await gameService.getUserGames(userId);
        res.status(200).json({ success: true, data: games });
    } catch (error) {
        res.status(400).json({ success: false, message: error.message });
    }
};

async function getGameDetails(req, res) {
    try {
        const gameId = req.params.gameId;
        const game = await gameService.getGameDetails(gameId);
        res.status(200).json({ success: true, data: game });
    } catch (error) {
        res.status(400).json({ success: false, message: error.message });
    }
};

async function getShopPlans(req, res) {
    try {
        const shopPlans = await shopPlanService.getShopPlans();
        res.status(200).json({ success: true, data: shopPlans });
    } catch (error) {
        res.status(400).json({ success: false, message: error.message });
    }
};

async function purchaseShopPlan(req, res) {
    try {
        const { planId } = req.body;
        const { userId } = req.user;
        const purchaseResult = await shopPlanService.purchaseShopPlan(userId, planId);
        res.status(200).json({ success: true, data: purchaseResult });
    } catch (error) {
        res.status(400).json({ success: false, message: error.message });
    }
};

module.exports = {
    signup_login, sendOTP, editProfile, profile,
    verify, getUserGames, getShopPlans, purchaseShopPlan,
    getGameDetails, userProfiles, getUserProfile, onlinePlayers
};
