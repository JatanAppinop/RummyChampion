const { required } = require('joi');
const mongoose = require('mongoose');

const settingsSchema = new mongoose.Schema({
    referralCommission: {
        type: Number,
        required: true
    },
    minWithdrawal: {
        type: Number,
        required: true
    },
    maxWithdrawal: {
        type: Number,
        required: true
    },
    perDayWithdrawal: {
        type: Number,
        required: true
    },
    bonus: {
        type: Number,
        required: true
    },
    maintenance: {
        type: Boolean,
        required: true
    },
    upiId: {
        type: String,
        required: true
    }
}, {
    timestamps: true,
    versionKey: false
});

const Settings = mongoose.model('Settings', settingsSchema);

module.exports = Settings;
