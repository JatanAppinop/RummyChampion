const mongoose = require('mongoose');
const { ObjectId } = mongoose.Schema.Types;
const errorHandler= require("../utils/CustomError.js");
// Define the KYC schema
const kycSchema = new mongoose.Schema(
    {
        // Reference to the User collection
        userId: { type: ObjectId, ref: 'User', required: true },

        // Aadhaar KYC details
        aadharKyc: {
            nameOnAadhar: { type: String },
            imageOnAadhar: { type: String },
            aadharNumber: { type: String },
            aadharAddress: { type: String },
            dob: { type: String },
            attemptCount: { type: Number, default: 0 },
            lastAttemptTime: { type: Date, default: Date.now },
            rejectReason: { type: String, default: "" },
            aadharStatus: {
                type: String,
                enum: ["Not Submitted", "Pending", "Approved", "Rejected"],
                default: "Not Submitted",
            },
        },

        // PAN KYC details
        panKyc: {
            nameOnPan: { type: String },
            panNumber: { type: String },
            attemptCount: { type: Number, default: 0 },
            lastAttemptTime: { type: Date, default: Date.now },
            rejectReason: { type: String, default: "" },
            panStatus: {
                type: String,
                enum: ["Not Submitted", "Pending", "Approved", "Rejected"],
                default: "Not Submitted",
            },
        },
    },
    {
        timestamps: true,
        toJSON: { virtuals: true },
        toObject: { virtuals: true },
    }
);

/// *** Instance Methods *** ///

/**
 * Updates the Aadhaar status and logs the last attempt.
 * @param {String} status - The new status for Aadhaar.
 * @param {String} [reason] - The reason for rejection (optional).
 */
kycSchema.methods.updateAadharStatus = function (status, reason = "") {
    this.aadharKyc.aadharStatus = status;
    this.aadharKyc.lastAttemptTime = new Date();

    if (status === "Rejected") {
        this.aadharKyc.rejectReason = reason;
        this.aadharKyc.attemptCount += 1;
    }
    return this.save();
};

/**
 * Updates the PAN status and logs the last attempt.
 * @param {String} status - The new status for PAN.
 * @param {String} [reason] - The reason for rejection (optional).
 */
kycSchema.methods.updatePanStatus = function (status, reason = "") {
    this.panKyc.panStatus = status;
    this.panKyc.lastAttemptTime = new Date();

    if (status === "Rejected") {
        this.panKyc.rejectReason = reason;
        this.panKyc.attemptCount += 1;
    }
    return this.save();
};

/**
 * Checks if both Aadhaar and PAN KYC are approved.
 * @returns {Boolean} - True if both are approved, otherwise false.
 */
kycSchema.methods.isKycComplete = function () {
    return (
        this.aadharKyc.aadharStatus === "Approved" &&
        this.panKyc.panStatus === "Approved"
    );
};

/// *** Static Methods *** ///

/**
 * Finds KYC records with pending Aadhaar or PAN status.
 * @returns {Promise<Array>} - An array of KYC records with pending statuses.
 */
kycSchema.statics.findPendingKyc = function () {
    return this.find({
        $or: [
            { "aadharKyc.aadharStatus": "Pending" },
            { "panKyc.panStatus": "Pending" },
        ],
    });
};

/**
 * Finds KYC records by user ID.
 * @param {ObjectId} userId - The ID of the user.
 * @returns {Promise<Object|null>} - The KYC record for the given user, or null if not found.
 */
kycSchema.statics.findByUserId = function (userId) {
    return this.findOne({ userId });
};

kycSchema.methods.validateAadharRequest = function (userId) {
    const kycData = KYC.findOne({ userId: userId });
    const MAX_ATTEMPTS = 5;
    const TIME_LIMIT = 24 * 60 * 1000;
    const currentTime = new Date();
    const lastAttemptTime = kycData?.aadharKyc?.lastAttemptTime || 0
    const timeDifference = currentTime - lastAttemptTime;
    if (kycData) {
        if (timeDifference > TIME_LIMIT) {
            KYC.updateOne({ userId }, { $set: { "aadharKyc.attemptCount": 1, "aadharKyc.lastAttemptTime": currentTime, 'aadharKyc.aadharStatus': kycData.aadharKyc.aadharStatus } });
            return 1
        }
        else {
            if (kycData.aadharKyc.attemptCount >= MAX_ATTEMPTS) {
                throw errorHandler("You have reached to max attempt. Please try again after 24 hour", 200);
            }
            KYC.updateOne({ userId }, { $set: { 'aadharKyc.lastAttemptTime': currentTime, 'aadharKyc.aadharStatus': kycData.aadharKyc.aadharStatus }, $inc: { 'aadharKyc.attemptCount': 1 } });
            return kycData.aadharKyc.attemptCount + 1
        }
    } else {
        let kycData = KYC.updateOne(
            { userId: userId },
            { $set: { 'aadharKyc.attemptCount': 1, ' aadharKyc.lastAttemptTime': currentTime, 'aadharKyc.aadharStatus': "Not Submitted" } },
            { upsert: true });
        if (kycData.upsertedCount > 0 || kycData.modifiedCount > 0) {
            return 1;
        } else {
            throw errorHandler("Something went wrong in KYC processing. Please try again after some time", 200);
        }
    }
};

// Create the KYC model
const KYC = mongoose.model('Kyc', kycSchema);

module.exports = KYC;
