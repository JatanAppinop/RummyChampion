const { required } = require("joi");
const mongoose = require("mongoose")


const SubadminSchema = new mongoose.Schema(
    {
        name: { type: String },
        email: { type: String },
        mobileNumber: { type: String },
        password: { type: String },
        dob: { type: String },
        userType: { type: Number, default: 0, required: true },
        gender: { type: String },
        permissions: { type: Array, default: [] },
        active: { type: String, default: "1" }
    },
    { timestamps: true }
);

const adminDetails = mongoose.model("adminDetails", SubadminSchema)

module.exports = adminDetails;