const notificationRepository = require('../repositories/notificationRepository');
const { sendSms, sendFcm } = require('../utils/Notifications')

class notificationService {
    async createBanner(url, image, bannerType, bannerSequence) {
        const bannerData = {
            url,
            bannerType,
            bannerSequence,
            image
        };

        return await notificationRepository.createBanner(bannerData);
    };

    async getActiveBanners() {
        return await notificationRepository.findAllActiveBanners();
    };

    async updateBannerStatus(bannerId, isActive) {
        return await notificationRepository.updateBanner(bannerId, { isActive });
    };

    async sendSmsNotification(userId, message) {
        const user = await notificationRepository.findUserById(userId);
        if (!user) throw new Error('User not found');

        const result = await sendSms(user.mobileNumber, message);
        await notificationRepository.logNotification(userId, message, 'sms', result.status);

        return result;
    };

    async sendFcmNotification(userId, message) {
        const user = await notificationRepository.findUserById(userId);
        if (!user) throw new Error('User not found');

        const result = await sendFcm(user.fcmToken, message);
        await notificationRepository.logNotification(userId, message, 'fcm', result.status);

        return result;
    };

}
module.exports = new notificationService()
