const { func } = require("joi");
const adminService = require("../services/adminService");
const dashboardService = require("../services/dashboardService")
const APK = require('../models/Apk')

async function login(req, res) {
    try {
        const { email, password } = req.body;
        const result = await adminService.loginAdmin(email, password);

        return res.status(200).json(result);
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
};

async function changePass(req, res) {
    const { email, oldPassword, newPassword } = req.body;
    console.log(email, oldPassword, newPassword);
    try {
        const result = await adminService.passwordChange(email, oldPassword, newPassword);
        return res.status(200).json({ success: true, message: "Password Changed successfully", data: result });
    } catch (error) {
        console.error('Error in sendOtp controller:', error);
        return res.status(500).json({ success: false, message: "Password Not changed", data: null });
    }
};

async function forgotPass(req, res) {
    try {
        const { email, password } = req.body;
        const result = await adminService.sendForgotPasswordEmail(email, password);

        return res.status(200).json({
            success: result.success,
            message: result.message,
            data: null
        });
    } catch (error) {
        return res.status(500).json({
            success: false,
            message: error.message,
            data: null
        });
    }
};

async function editProfile(req, res) {
    const { userId } = req.user;
    try {
        const userData = req.body;
        const result = await adminService.updateProfile(userId, userData);
        res.status(200).json(result);
    } catch (error) {
        console.error('Error in updating profile:', error);
        return res.status(500).json({ success: false, message: "Failed to update Profile", data: null });
    }
};

async function getAllUsers(req, res) {
    try {
        const users = await adminService.getUsers();
        res.status(200).json(users);
    } catch (error) {
        console.error('Error in updating profile:', error);
        return res.status(500).json({ success: false, message: "Failed to update Profile", data: null });
    }
};

async function blockUser(req, res) {
    try {
        const { status, userId } = req.body;

        if (status !== "Active" && status !== "Inactive") {
            return res.status(400).json({ success: false, message: "Invalid status value" });
        }

        await adminService.updateUsersStatus(userId, status);

        return res.status(200).json({ success: true, message: `User status set to ${status}` });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.stack });
    }
};

async function blockAllUser(req, res) {
    try {
        const { status } = req.body;

        if (status !== "Active" && status !== "Inactive") {
            return res.status(400).json({ success: false, message: "Invalid status value" });
        }

        await adminService.updateAllUsersStatus(status);

        return res.status(200).json({ success: true, message: `All users set to ${status}` });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message });
    }

};

async function dashboard(req, res) {
    try {
        const data = await dashboardService.fetchDashboardStats();
        res.status(200).json({ success: true, data, message: "dashboard fetched" });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
};

async function uploads(req, res) {
    try {
        let addapk = await APK.create({
            apkfile: req.file.path
        })
        return res
            .status(200)
            .json({ success: true, message: "Uploaded", data: req.file.path });
    } catch (error) {
        return res
            .status(500)
            .json({ success: false, message: error.message, data: [] });
    }

};



module.exports = {
    login, changePass, forgotPass, editProfile, getAllUsers,
    blockUser, blockAllUser, dashboard, uploads
};
