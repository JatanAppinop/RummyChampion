const { ObjectId } = require("bson");
const mongoose = require("mongoose");

const otpSchema = new mongoose.Schema(
    {
        mobileNumber: { type: String },
        otp: { type: Number },
        createdAt: { type: Date, expires: '4m', default: Date.now }
    },
    { timestamps: true }
);

const Otp = mongoose.model("Otp", otpSchema);

module.exports = Otp;