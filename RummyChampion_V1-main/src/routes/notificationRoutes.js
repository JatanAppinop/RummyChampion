const express = require('express');
const router = express.Router();
const { Auth } = require('../middlewares/Auth');
const { uploadBanner, getBanners, updateBanner, listUsers,
    sendSmsNotification, sendFcmNotification, listNotifications, bannersUser
} = require('../controllers/notificationController');
const upload = require('../middlewares/Upload')


router.post('/admin/uploadBanner', [Auth, upload.fields([{ name: 'image' }])], uploadBanner);

router.get('/admin/bannerList', [Auth], getBanners);

router.put('/admin/updateBanner', [Auth], updateBanner);

router.get('/admin/notifyUsers', [Auth], listUsers);

router.post('/admin/notify/sms', [Auth], sendSmsNotification);

router.post('/admin/notify/fcm', [Auth], sendFcmNotification);

router.get('/admin/notifications', [Auth], listNotifications);

router.get('/user/banners', [Auth], bannersUser)

module.exports = router;
