const adminRepository = require("../repositories/adminRepository")
const bcrypt = require("../utils/Bcrypt");
const jwt = require("../utils/Jwt");
const { JWT_SECRET, ACCESS_TOKEN_EXPIRY } = process.env

class adminService {
    async loginAdmin(email, password) {
        try {
            console.log('Attempting to find subadmin with email:', email);
            const subadmin = await adminRepository.findByEmail(email);

            if (!subadmin) {
                console.log('No user found for email:', email);
                return { success: false, message: 'No user found', data: [] };
            }

            if (subadmin.active === '0') {
                console.log('Subadmin is not active:', email);
                return { success: false, message: 'Subadmin is not active', data: [] };
            }

            console.log('Comparing passwords...');
            const compare = await bcrypt.passwordComparison(password, subadmin.password);
            console.log('Password comparison result:', compare);

            if (!compare) {
                console.log('Incorrect password for email:', email);
                return { success: false, message: 'Incorrect password', data: [] };
            }

            const perArr = subadmin.permissions.map((a) => a);
            console.log('Permissions:', perArr);

            const jwtData = {
                email: subadmin.email,
                name: subadmin.name,
                userId: subadmin._id
            };

            const accessToken = await jwt.generateToken(jwtData, JWT_SECRET,
                ACCESS_TOKEN_EXPIRY);

            const userData = {
                _id: subadmin._id,
                email: subadmin.email,
                name: subadmin.name,
                permissions: perArr,
                userType: subadmin.email === 'admin@rc.com' ? 1 : 0,
                accessToken,
            };

            console.log('Login successful for email:', email);

            return { success: true, data: userData, message: 'Login successful' };

        } catch (error) {
            console.error('Login error:', error);
            return { success: false, message: 'An error occurred during login', data: [] };
        }
    }

    async passwordChange(email, oldPassword, newPassword) {
        const user = await adminRepository.findByEmail(email);
        if (!user) {
            throw new Error('User not found');
        }
        const compare = await bcrypt.passwordComparision(oldPassword, user.password);

        if (compare) {
            await adminRepository.updatePassword(email, newPassword);
            return { success: true, message: 'Password changed successfully' };
        } else {
            throw new Error('Old password does not match');
        }
    }

    async sendForgotPasswordEmail(email, password) {
        if (!email) {
            throw new Error('Please provide an email address');
        }

        const user = await adminRepository.findByEmail(email);

        if (!user) {
            throw new Error('Email address not found');
        }

        await validators.emailValidation(email);

        // const newPassword = `RC@${crypto.randomInt(0, 1000000)}`;
        const encryptedPassword = await bcrypt.passwordEncryption(password);

        await adminRepository.updatePassword(email, encryptedPassword);

        // await SendMail.forgotPassword(email, newPassword);

        return { success: true, message: 'Password sent to your email address' };
    }

    async updateProfile(userId, userData) {
        try {
            const existingUser = await adminRepository.findById(userId);
            if (!existingUser) {
                return { success: false, message: "User not found!", data: [] };
            }

            await adminRepository.updateProfile(userId, userData);
            return { success: true, message: "Profile updated.", data: userData };
        } catch (error) {
            return { success: false, message: error.message, data: [] };
        }
    }

    async getUsers() {
        try {
            const usersData = await adminRepository.findUsers();

            return { success: true, message: "All users updated data.", data: usersData };
        } catch (error) {
            return { success: false, message: error.message, data: [] };
        }
    }

    async updateUsersStatus(userId, status) {
        try {
            await adminRepository.updateUsersStatus(userId, status);
        } catch (error) {
            throw new Error(`Failed to update users' status: ${error.message}`);
        }
    }

    async updateAllUsersStatus(status) {
        try {
            await adminRepository.updateAllUsersStatus(status);
        } catch (error) {
            throw new Error(`Failed to update all users' status: ${error.message}`);
        }
    }

}
module.exports = new adminService();