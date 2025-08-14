const User = require("../models/User");
const otp = require("../models/Otp")

class UserRepository {
    async findOneByMobileNumber(mobileNumber) {
        return await User.findOne({ mobileNumber }).lean();
    }

    async createUser(userData) {
        return await User.create(userData);
    }

    async update(userId, updateData) {
        return await User.findByIdAndUpdate(userId, updateData, { new: true });
    }

    async findOtp(mobileNumber) {
        return await otp.findOne({ mobileNumber });
    }

    async updateOtp(mobileNumber, otps) {
        await otp.findOneAndUpdate(
            { mobileNumber: mobileNumber },
            { $set: { otp: otps } },
            { upsert: true, new: true }
        );
    }

    async findById(id) {
        return await User.findById(id).lean();
    }

    async findById(userId) {
        return await User.findById(userId);
    }

    async updateBalance(userId, winningAmount) {
        return await User.findByIdAndUpdate(userId, {
            $inc: { winningBalance: winningAmount, totalBalance: winningAmount }
        }, { new: true });
    }

    async updateProfile(id, userData) {
        return await User.updateOne({ _id: id }, { $set: userData });
    }

    async findByUsername(username) {
        return await User.findOne({ username: { $regex: new RegExp(`^${username}$`, 'i') } });
    }

    async updateUser(user, session) {
        await user.save({ session });
    }

    async findByUserId(userId, session) {
        return await User.findById(userId).session(session);
    }

}


module.exports = new UserRepository();
