const mongoose = require('mongoose');

const bannerSchema = new mongoose.Schema({
    url: {
        type: String,
        required: true
    },
    image: {
        type: String, // URL or path to the image file
        required: true
    },
    createdOn: {
        type: Date,
        default: Date.now
    },
    isActive: {
        type: Boolean,
        default: true
    },
    bannerType: {
        type: String
    },
    bannerSequence: {
        type: Number
    }
}, {
    timestamps: true,
    versionKey: false
});

const Banner = mongoose.model('Banner', bannerSchema);

module.exports = Banner;
