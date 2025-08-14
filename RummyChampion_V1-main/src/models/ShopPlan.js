const mongoose = require('mongoose');

const shopPlanSchema = new mongoose.Schema({
    planName: {
        type: String,
        required: true
    },
    price: {
        type: Number,
        required: true
    },
    coin: {
        type: Number,
        required: true
    },
    description: {
        type: String,
        required: true
    },
    createdOn: {
        type: Date,
        default: Date.now
    },
    isActive: {
        type: Boolean,
        default: true
    }
}, {
    timestamps: true,
    versionKey: false
});

const ShopPlan = mongoose.model('ShopPlan', shopPlanSchema);

module.exports = ShopPlan
