const notificationService = require('../services/notificationService');
const notificationRepisitory = require('../repositories/notificationRepository')

async function uploadBanner(req, res) {
    try {
        const { url, bannerType, bannerSequence } = req.body;
        const image = req.files.image[0].path;
        const banner = await notificationService.createBanner(url, image, bannerType, bannerSequence);
        res.status(201).json({ message: 'Banner uploaded successfully', banner });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
};

async function getBanners(req, res) {
    try {
        const banners = await notificationService.getActiveBanners();
        res.status(200).json({ message: "Banners fetched successfully", banners });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
};

async function updateBanner(req, res) {
    try {
        const { bannerId, isActive } = req.body;
        const updatedBanner = await notificationService.updateBannerStatus(bannerId, isActive);
        res.status(200).json({ success: true, message: 'Banner status updated successfully', updatedBanner });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
};

async function listUsers(req, res) {
    try {
        const users = await notificationRepisitory.findAllUsers();
        res.status(200).json({ success: true, users });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
};

async function sendSmsNotification(req, res) {
    try {
        const { userId, message } = req.body;
        const result = await notificationService.sendSmsNotification(userId, message);
        res.status(200).json({ sucess: true, result });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
};

async function sendFcmNotification(req, res) {
    try {
        const { userId, message } = req.body;
        const result = await notificationService.sendFcmNotification(userId, message);
        res.status(200).json({ success: true, result });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
};

async function listNotifications(req, res) {
    try {
        const notifications = await notificationRepisitory.findAllNotifications();
        res.status(200).json({ success: true, notifications });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
};

async function bannersUser(req, res) {
    try {
        const data = await notificationService.getActiveBanners()
        res.status(200).json({ success: true, data });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
}


module.exports = {
    uploadBanner, getBanners, updateBanner, bannersUser,
    listUsers, sendSmsNotification, sendFcmNotification, listNotifications
};
