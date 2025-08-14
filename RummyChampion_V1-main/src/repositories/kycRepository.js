const mongoose = require('mongoose')
const Kyc = require('../models/Kyc');
const User = require('../models/User')
const Bank = require('../models/Banks')
const FundAccount = require('../models/FundAccount')


class kycRepository {

    async createKyc(kycData) {
        return await Kyc.create(kycData);
    }

    async findByUserId(userId) {
        return await Kyc.findOne({ userId: userId });
    }

    async findByDocumentNumber(documentNumber) {
        return await Kyc.findOne({ documentNumber: documentNumber });
    }

    async updateUserKycStatus(userId) {
        return await User.updateOne({ _id: userId }, { $set: { kycSubmitted: true, kycVerified: "Pending" } });
    }

    async findUserById(userId) {
        return await User.findById(userId);
    }

    async findKyc() {
        return await Kyc.find();
    }

    async updateKycStatus(kycVerified, userId, reason) {
        // const session = await mongoose.startSession();
        // session.startTransaction();
        try {
            const userUpdate = await User.findOneAndUpdate(
                { _id: userId },
                { $set: { kycVerified: kycVerified } },
                { new: true, }
            );

            const kycUpdate = await Kyc.findOneAndUpdate(
                { userId: userId },
                { $set: { verified: kycVerified, rejectReason: reason } },
                { new: true,  }
            );

            // await session.commitTransaction();

            return { userUpdate, kycUpdate };
        } catch (error) {
            // await session.abortTransaction();
            throw error;
        } 
        // finally {
        //     session.endSession();
        // }
    }

    async findBanks() {
        return await Bank.find();
    }

    async updateBanks(userId, verified) {
        return await Bank.findOneAndUpdate({ userId: userId }, { verified: verified })
    }

    async findKycByStatus(status) {
        return await Kyc.find({ verified: status });
    }

    async findBankByStatus(status) {
        return await Bank.find({ verified: status });
    }

    async getFunds(userId) {
        return await FundAccount.find({ userId: userId })

    }
}
module.exports = new kycRepository()