// services/fcmService.js
const admin = require('firebase-admin');

// Ensure Firebase Admin SDK is initialized
admin.initializeApp({
    credential: admin.credential.applicationDefault()
});

const sendFcm = async (fcmToken, message) => {
    try {
        const result = await admin.messaging().send({
            token: fcmToken,
            notification: {
                title: 'Notification',
                body: message
            }
        });
        return { status: 'Sent', detail: 'Message sent successfully' };
    } catch (error) {
        return { status: 'Failed', detail: error.message };
    }
};

// services/smsService.js
const sendSms = async (phoneNumber, message) => {
    // Implement SMS sending logic here
    // Example: Use Twilio or any other SMS provider
    try {
        // Example with Twilio
        // const result = await twilioClient.messages.create({
        //     body: message,
        //     from: 'YOUR_TWILIO_NUMBER',
        //     to: phoneNumber
        // });
        return { status: 'Sent', detail: 'Message sent successfully' };
    } catch (error) {
        return { status: 'Failed', detail: error.message };
    }
};


module.exports = {
    sendFcm, sendSms
};
