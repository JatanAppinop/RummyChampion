const { ObjectId } = require("bson");
const mongoose = require("mongoose");

const PayProfileSchema = new mongoose.Schema(
    {
        contactId: {
            type: String
        },
        entity: {
            type: String
        },
        name: {
            type: String
        },
        contact: {
            type: Number
        },
        email: {
            type: String
        },
        type: {
            type: String
        },
        userId: {
            type: ObjectId,
            ref: "User"
        },
        batchId: {
            type: String
        },
        active: {
            type: Boolean
        },
        notes: {
            type: Array
        },
        createdAt: {
            type: Number
        },
    },
    {
        timestamps: true,
    }
);

const PayProfile = mongoose.model("PayProfile", PayProfileSchema);

module.exports = PayProfile
