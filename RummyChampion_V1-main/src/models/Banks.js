const { ObjectId } = require('bson');
const mongoose = require('mongoose');
const User = require('./User')


const userBankSchema = new mongoose.Schema(
    {
        userId: { type: ObjectId, required: false, },
        beneficaryName: { type: String, required: true },
        accountNumber: { type: String, required: true },
        mobileNumber: { type: String, required: true },
        ifscCode: { type: String, required: true },
        verified: { type: "String", default: "Pending", required: true },
    },
    /**
     * status for verification 
     * Pending
     * Approved
     * NotApplied
     */
    { timestamps: true }
)

userBankSchema.virtual("user", {
    ref: User,
    localField: "userId",
    foreignField: "_id",
    justOne: true,
});

const bankdetails = mongoose.model("bankdetails", userBankSchema);

module.exports = bankdetails;