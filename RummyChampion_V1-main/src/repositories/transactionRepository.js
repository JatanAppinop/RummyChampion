const mongoose = require("mongoose");
const Banks = require("../models/Banks");
const User = require("../models/User");
const Transaction = require("../models/Transaction");

class transactionRepository {
  async userBank(userId) {
    return await Banks.find({ userId: userId });
  }

  async createUserBank(userData) {
    return await Banks.create(userData);
  }

  async userWallet(userId) {
    return await User.findOne(
      { _id: userId },
      {
        username: 1,
        totalBalance: 1,
        winningBalance: 1,
        depositBalance: 1,
        cashBonus: 1,
        bonus: 1,
      }
    );
  }

  async createWallet(userId, balance) {
    return await User.updateOne(
      { _id: userId },
      {
        $set: {
          cashBonus: balance.cashBonus,
          depositBalance: balance.depositBalance,
        },
      }
    );
  }
  async userWalletBalance(userId, amount) {
    return await User.updateOne(
      { _id: userId },
      {
        $inc: {
          depositBalance: amount,
        },
      }
    );
  }

  async createTransaction(transactionData, session) {
    const transaction = new Transaction(transactionData);
    return await transaction.save({ transactionData });
  }

  async findTransByStatus(status) {
    return await Transaction.find({
      status,
      transactionType: "withdrawal",
    });
  }

  async findByTransactionId(requestId, session) {
    return await Transaction.findById(requestId).session(session);
  }

  async updateData(transaction, session) {
    await transaction.save({ session });
  }

  async findTransById(userId) {
    return await Transaction.find({ userId: userId }).sort({ createdAt: -1 });
  }

  async findAllTrans() {
    return await Transaction.find({});
  }

  async requestDeposit(userId, amount, screenshot) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      const user = await User.findById(userId);
      if (!user) {
        throw { statusCode: 404, message: "User not found" };
      }

      const screenshotPath = screenshot ? screenshot.path : "";

      const transactionData = {
        userId,
        amount,
        status: "Pending",
        title: "Deposit Request",
        description: `Requested deposit of ${amount}`,
        transactionType: "deposit",
        transactionInto: "user",
        screenshot: screenshotPath,
      };

      const transaction = await Transaction.create([transactionData], {
        session,
      });

      await session.commitTransaction();
      session.endSession();

      return transaction;
    } catch (error) {
      await session.abortTransaction();
      session.endSession();
      throw { statusCode: error.statusCode || 500, message: error.message };
    }
  }

  async requestWithdrawal(userId, amount, upiId) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      const user = await User.findById(userId);
      if (!user) {
        throw { statusCode: 404, message: "User not found" };
      }

      if (user.totalBalance < amount) {
        throw { statusCode: 400, message: "Insufficient balance" };
      }

      const transactionData = {
        userId,
        amount,
        status: "Pending",
        title: "Withdrawal Request",
        description: `Requested withdrawal of ${amount} to UPI ID ${upiId}`,
        transactionType: "withdrawal",
        transactionInto: "user",
        upiId,
      };

      const transaction = await Transaction.create([transactionData], {
        session,
      });

      await session.commitTransaction();
      session.endSession();

      return transaction;
    } catch (error) {
      await session.abortTransaction();
      session.endSession();
      throw { statusCode: error.statusCode || 500, message: error.message };
    }
  }

  async approveDeposit(userId, transactionId) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      const transaction = await Transaction.findById(transactionId).session(
        session
      );
      if (
        !transaction ||
        transaction.userId.toString() !== userId.toString() ||
        transaction.status !== "Pending"
      ) {
        throw {
          statusCode: 404,
          message: "Transaction not found or already processed",
        };
      }

      const user = await User.findById(userId).session(session);
      user.depositBalance += transaction.amount;
      await user.save({ session });

      transaction.status = "Approved";
      await transaction.save({ session });

      await session.commitTransaction();
      session.endSession();

      return transaction;
    } catch (error) {
      await session.abortTransaction();
      session.endSession();
      throw { statusCode: error.statusCode || 500, message: error.message };
    }
  }

  async approveWithdrawal(userId, transactionId) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      const transaction = await Transaction.findById(transactionId).session(
        session
      );
      if (
        !transaction ||
        transaction.userId.toString() !== userId.toString() ||
        transaction.status !== "Pending"
      ) {
        throw {
          statusCode: 404,
          message: "Transaction not found or already processed",
        };
      }

      const user = await User.findById(userId).session(session);
      if (user.winningBalance < transaction.amount) {
        throw { statusCode: 400, message: "Insufficient balance" };
      }

      user.winningBalance -= transaction.amount;
      await user.save({ session });

      transaction.status = "Approved";
      await transaction.save({ session });

      await session.commitTransaction();
      session.endSession();

      return transaction;
    } catch (error) {
      await session.abortTransaction();
      session.endSession();
      throw { statusCode: error.statusCode || 500, message: error.message };
    }
  }

  async getApprovedDeposit() {
    return await Transaction.find({
      $and: { status: "Approved", transactionType: "deposit" },
    });
  }

  async getPendingDeposit() {
    return await Transaction.find({
      $and: { status: "Pending", transactionType: "deposit" },
    });
  }

  async findByPeTransactionId(merchantTransactionId) {
    return await Transaction.findOne({
      merchantTransactionId: merchantTransactionId,
    });
  }
  async findByPeTransactionIdForUser(userId, merchantTransactionId) {
    return await Transaction.findOne({
      userId: new mongoose.Types.ObjectId(userId),
      merchantTransactionId: merchantTransactionId,
    });
  }

  async updateTransaction(transaction) {
    return await transaction.save();
  }
}

module.exports = new transactionRepository();
