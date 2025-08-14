const mongoose = require('mongoose')
const shopPlanRepository = require("../repositories/shopPlanRepository")
const transactionRepository = require("../repositories/transactionRepository")
const userRepository = require("../repositories/userRepository")

class shopPlanService {

    async getAllShopPlans() {
        return await shopPlanRepository.getAllShopPlans();
    }

    async createShopPlan(shopPlanData) {
        return await shopPlanRepository.createShopPlan(shopPlanData);
    }

    async getShopPlans() {
        return await shopPlanRepository.getShopPlans();
    }

    async purchaseShopPlan(userId, planId) {
        const session = await mongoose.startSession();
        session.startTransaction();
        try {
            // Validate the user
            const user = await userRepository.findById(userId);
            if (!user) {
                throw { statusCode: 404, message: 'User not found' };
            }

            // Validate the shop plan
            const shopPlan = await shopPlanRepository.getShopPlanById(planId);
            if (!shopPlan || !shopPlan.isActive) {
                throw { statusCode: 404, message: 'Shop plan not found or inactive' };
            }

            const previousTotalBalance = user.totalBalance;
            const currentTotalBalance = previousTotalBalance - shopPlan.price;

            if (currentTotalBalance < 0) {
                throw { statusCode: 400, message: 'Insufficient balance' };
            }

            // Update user's total balance
            user.totalBalance = currentTotalBalance;
            await userRepository.updateUser(user, session);

            // Create a new transaction
            const transactionData = {
                userId: user._id,
                amount: shopPlan.price,
                previousWinningBalance: user.winningBalance,
                previousTotalBalance: previousTotalBalance,
                previousCashBonus: user.cashBonus,
                currentWinningBalance: user.winningBalance,
                currentTotalBalance: currentTotalBalance,
                currentCashBonus: user.cashBonus,
                status: 'Approved',
                title: `Purchased ${shopPlan.planName}`,
                description: shopPlan.description,
                transactionType: 'Shop-Purchase',
                transactionInto: 'Plan-Purchase',
                isDeductTds: false,
                numericId: 0 // This will be set in pre-save hook
            };

            const transaction = await transactionRepository.createTransaction(transactionData, session);

            // Commit the transaction
            await session.commitTransaction();
            session.endSession();

            return { message: 'Shop plan purchased successfully', transaction };
        } catch (error) {
            await session.abortTransaction();
            session.endSession();
            throw error;
        }
    }

}

module.exports = new shopPlanService();