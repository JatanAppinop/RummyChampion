const mongoose = require('mongoose');

const notificationLogSchema = new mongoose.Schema({
    userId: {
        type: mongoose.Schema.Types.ObjectId,
        ref: 'User',
        required: true
    },
    message: {
        type: String,
        required: true
    },
    method: {  // 'sms' or 'fcm'
        type: String,
        required: true
    },
    status: {
        type: String,
        default: 'Sent'
    },
    createdOn: {
        type: Date,
        default: Date.now
    }
}, {
    timestamps: true,
    versionKey: false
});

const NotificationLog = mongoose.model('NotificationLog', notificationLogSchema);

module.exports = NotificationLog;
