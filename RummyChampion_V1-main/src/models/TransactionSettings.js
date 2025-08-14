const mongoose = require('mongoose');

const transactionSettingsSchema = new mongoose.Schema({
    sgst: {
        type: Number,
        required: true,
        default: 0
    },
    cgst: {
        type: Number,
        required: true,
        default: 0
    },
    tds: {
        type: Number,
        required: true,
        default: 0
    },
    withdrawCharge: {
        type: Number,
        required: true,
        default: 0
    },
    bonus: {
        type: Number,
        required: true,
        default: 0
    }
}, {
    timestamps: true,
    versionKey: false
});

const TransactionSettings = mongoose.model('TransactionSettings', transactionSettingsSchema);

module.exports = TransactionSettings;
