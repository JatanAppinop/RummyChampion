const mongoose = require("mongoose")
const { ObjectId } = require("bson");

const FundAccountSchema = new mongoose.Schema({

    userId: {
        type: ObjectId,
        ref: "User"
    },
    fundAccountId: {
        type: String,
        ref: "PayProfile"
    },
    entity: {
        type: String
    },
    contactId: {
        type: String,
        ref:"PayProfile"
    },
    accountType: {
        type: String
    },
    bankAccount: {
        ifsc: { type: String },
        bankName: { type: String },
        name: { type: String },
        notes: [],
        account_number: { type: String }
    },
    vpa: {
        address: { type: String }
    },
    batchId: {
        type: String
    },
    active: {
        type: Boolean
    },
    createdAt: {
        type: Number
    }

}, {
    timestamps: true,
})


const FundAccount = mongoose.model("FundAccount", FundAccountSchema)

module.exports = FundAccount