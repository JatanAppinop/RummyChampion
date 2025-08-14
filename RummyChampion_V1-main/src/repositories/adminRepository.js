const AdminDetails = require('../models/Admins');
const User = require('../models/User')
const bcrypt = require("../utils/Bcrypt")

class adminRepository {
    async findByEmail(email) {
        return AdminDetails.findOne({ email: email });
    }

    async updatePassword(email, newPassword) {
        const bcryptNewPassword = await bcrypt.passwordEncryption(newPassword);
        return await AdminDetails.updateOne({ email: email }, { $set: { password: bcryptNewPassword } });
    }

    async findById(userId) {
        return await AdminDetails.findById(userId).lean();
    }

    async updateProfile(userId, userData) {
        return await AdminDetails.updateOne({ _id: userId }, { $set: userData });
    }

    async findUsers() {
        return await User.find();
    }

    async updateUsersStatus(userId, status) {
        await User.findByIdAndUpdate({ _id: userId }, { status: status }, { upsert: true });

    }

    async updateAllUsersStatus(status) {
        return await User.updateMany({}, { status });
    }
}

module.exports = new adminRepository()
