const Banner = require('../models/Banner');
const User = require('../models/User')
const NotificationLog = require('../models/Notification')


class notificationRepisitory {

    async createBanner(bannerData) {
        const banner = new Banner(bannerData);
        return await banner.save();
    };

    async findAllActiveBanners() {
        return await Banner.find({});
    };

    async updateBanner(bannerId, updateData) {
        return await Banner.findByIdAndUpdate(bannerId, updateData, { new: true });
    };

    async findAllUsers() {
        return await User.find({ isActive: true });
    };

    async findUserById(userId) {
        return await User.findById(userId);
    };

    async logNotification(userId, message, method, status) {
        const log = new NotificationLog({ userId, message, method, status });
        return await log.save();
    };

    async findAllNotifications() {
        return await NotificationLog.find().populate('userId', 'name email phoneNumber');
    };

}

module.exports = new notificationRepisitory()