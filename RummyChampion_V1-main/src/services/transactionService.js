const transactionRepository = require("../repositories/transactionRepository");
const mongoose = require("mongoose");
const User = require("../models/User");
const userRepository = require("../repositories/userRepository");
const GSTInvoice = require("../models/Gst");
const TransactionSettings = require("../models/TransactionSettings");
const {
  initiatePhonePeMobileSDK,
  verifyCallbackRequest,
  getTransactionStatus,
} = require("../gateways/phonePeGateway");
const { createOrderWithRazorpay } = require("../gateways/razorpayGateway");
const Settings = require("../models/Settings");
const FundAccount = require("../models/FundAccount");
const { createPayout } = require("../gateways/payoutGateway");

class transactionService {
  async bankDetail(userId) {
    try {
      let details = await transactionRepository.userBank(userId);

      if (details != null) {
        return details;
      } else {
        return {
          sucess: false,
          message: "No Bank Details Found ",
          data: details,
        };
      }
    } catch (err) {
      return { success: false, message: err.message, data: [] };
    }
  }

  async addBank(userId, bankData) {
    try {
      // Retrieve existing bank accounts count for the user
      const existingAccountsCount = await transactionRepository.userBank(
        userId
      );
      console.log(existingAccountsCount);

      // Check if the user has reached the account limit (3 accounts)
      if (existingAccountsCount.length > 3) {
        throw new Error("User has reached the account limit (3)");
      }

      // Check if the provided account number already exists for the user
      const existingAccount = existingAccountsCount.find(
        (account) => account.accountNumber === bankData.accountNumber
      );
      if (existingAccount) {
        throw new Error("Account Details already exist");
      }

      // Prepare new bank account data
      const userData = {
        userId: userId,
        mobileNumber: bankData.mobileNumber,
        ifscCode: bankData.ifscCode,
        accountNumber: bankData.accountNumber,
        beneficaryName: bankData.beneficaryName,
      };

      // Create the new bank account entry
      const newBankAccount = await transactionRepository.createUserBank(
        userData
      );
      console.log(newBankAccount, ">>>>>>>>>>>");

      return { data: userData }; // Return the added bank account data
    } catch (error) {
      throw new Error(`Failed to add bank account: ${error.message}`);
    }
  }

  async getWallet(userId) {
    try {
      let user = await transactionRepository.userWallet(userId);
      let minWithdrawal = await Settings.findOne();
      if (!user) {
        throw new Error("User not found");
      }

      // Return user wallet details
      return {
        totalBalance: user.totalBalance || 0,
        winningBalance: user.winningBalance || 0,
        depositBalance: user.depositBalance || 0,
        cashBonus: user.cashBonus || 0,
        bonus: user.bonus || 0,
        minimumWithdraw: minWithdrawal.minWithdrawal || 0,
        maximumWithdraw: minWithdrawal.maxWithdrawal || 0,
      };
    } catch (error) {
      throw new Error(`Failed to get user wallet: ${error.message}`);
    }
  }

  async updateWallet(userId, balance) {
    try {
      // Fetch the user from the database
      let user = await transactionRepository.userWallet(userId);

      // If user not found, throw error or handle appropriately
      if (!user) {
        throw new Error("User not found");
      }

      // Save the updated user
      await transactionRepository.createWallet(userId, balance);

      // Return the updated wallet details
      return await transactionRepository.userWallet(userId);
    } catch (error) {
      throw new Error(`Failed to update user wallet: ${error.message}`);
    }
  }
  async updateUserWallet(userId, balance) {
    try {
      // Fetch the user from the database
      let user = await transactionRepository.userWallet(userId);

      // If user not found, throw error or handle appropriately
      if (!user) {
        throw new Error("User not found");
      }

      // Save the updated user
      await transactionRepository.userWalletBalance(userId, balance);

      // Return the updated wallet details
      return await transactionRepository.userWallet(userId);
    } catch (error) {
      throw new Error(`Failed to update user wallet: ${error.message}`);
    }
  } 

  async requestWithdrawal(userId, amount, fundAccountId, accountType) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      const user = await userRepository.findById(userId);
      if (!user) {
        throw { statusCode: 404, message: "User not found" };
      }

      const previousTotalBalance = user.winningBalance;
      const currentTotalBalance = previousTotalBalance - amount;

      if (currentTotalBalance < 0) {
        throw { statusCode: 400, message: "Insufficient balance" };
      }
      const tdsGst = await TransactionSettings.findOne({});

      // Calculate GST and TDS
      const gstPercentage = tdsGst.totalGstPercentage; // Example value, replace with actual logic or configuration
      const tdsPercentage = tdsGst.tdsPercentage; // Example value, replace with actual logic or configuration
      // const gstAmount = (amount * gstPercentage) / 100;
      const tdsAmount = (amount * tdsPercentage) / 100;
      const netAmount = amount - tdsAmount;

      // Update user's total balance
      user.winningBalance = currentTotalBalance;
      await userRepository.updateUser(user, session);

      // Create a new transaction
      const transactionData = {
        userId: user._id,
        amount: amount,
        previousWinningBalance: user.winningBalance,
        previousTotalBalance: previousTotalBalance,
        previousCashBonus: user.cashBonus,
        currentWinningBalance: user.winningBalance,
        currentTotalBalance: currentTotalBalance,
        currentCashBonus: user.cashBonus,
        status: "Pending",
        title: "Withdrawal Request",
        description: `Requested withdrawal of ${amount}`,
        transactionType: "withdrawal",
        transactionInto: "user",
        isDeductTds: true,
        // gstDetails: {
        //     gstAmount: gstAmount, totalGstPercentage: gstPercentage
        // },
        tdsDetails: { tdsAmount: tdsAmount, tdsPercentage: tdsPercentage },
        netAmount: netAmount.toFixed(2),
        numericId: 0, // This will be set in pre-save hook
      };

      const transaction = await transactionRepository.createTransaction(
        transactionData,
        session
      );

      // const gstInvoiceData = {
      //     invoiceNo: `INV${Date.now()}`,
      //     invoiceDate: new Date(),
      //     userId: user._id,
      //     name: user.username,
      //     mobileNumber: user.mobileNumber,
      //     items: [
      //         {
      //             description: 'Withdrawal Transaction',
      //             quantity: 1,
      //             price: amount,
      //             gstRate: gstPercentage
      //         }
      //     ],
      //     amount: amount,
      //     gstAmount: gstAmount,
      //     stateGstAmount: gstAmount / 2,
      //     totalAmountWithGST: amount + gstAmount
      // };

      // // const account = await FundAccount.findOne({ userId: userId })

      // const gstInvoice = new GSTInvoice(gstInvoiceData);
      // await gstInvoice.save({ session });

      // Create Razorpay payout
      const payoutResponse = await createPayout({
        amount: netAmount,
        fundAccountId: fundAccountId,
        accountType: accountType,
      });

      // Update the transaction status to completed and add payout details
      transaction.status = "Completed";
      transaction.payoutDetails = payoutResponse;
      await transaction.save({ session });

      // Commit the transaction
      await session.commitTransaction();
      session.endSession();

      return transaction;
    } catch (error) {
      await session.abortTransaction();
      session.endSession();
      throw { statusCode: error.statusCode || 500, message: error.message };
    }
  }

  async handleWithdrawalRequest(requestId, status) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      const transaction = await transactionRepository.findByTransactionId(
        requestId,
        session
      );
      if (!transaction) {
        throw { statusCode: 404, message: "Transaction not found" };
      }

      if (status !== "Approved" && status !== "Rejected") {
        throw { statusCode: 400, message: "Invalid status" };
      }

      transaction.status = status;
      await transactionRepository.updateData(transaction, session);

      // If rejected, refund the amount back to user
      if (status === "Rejected") {
        const user = await userRepository.findById(transaction.userId, session);
        user.totalBalance += transaction.amount;
        await userRepository.updateUser(user, session);
      }

      await session.commitTransaction();
      session.endSession();

      return {
        message: `Withdrawal request ${status.toLowerCase()} successfully`,
        transaction,
      };
    } catch (error) {
      await session.abortTransaction();
      session.endSession();
      throw { statusCode: error.statusCode || 500, message: error.message };
    }
  }

  async getTransByStatus(status) {
    try {
      const transactions = await transactionRepository.findTransByStatus(
        status
      );
      return transactions;
    } catch (error) {
      console.error("Error fetching Transactions by status:", error);
      throw error;
    }
  }

  async handleDeposit(userId, amount) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      console.log(
        `Starting deposit for userId: ${userId} with amount: ${amount}`
      );
      const user = await userRepository.findById(userId);
      if (!user) {
        console.error("User not found");
        throw { statusCode: 404, message: "User not found" };
      }

      const previousTotalBalance = user.totalBalance; // Ensure previousTotalBalance is defined

      // Initiate PhonePe Payment
      const paymentResponse = await initiatePhonePePayment(user, amount);
      console.log("Payment response:", paymentResponse);
      if (!paymentResponse.success) {
        console.error("Payment initiation failed");
        throw { statusCode: 400, message: "Payment initiation failed" };
      }

      // Create a new transaction
      const transactionData = {
        userId: user._id,
        amount: amount,
        previousTotalBalance: previousTotalBalance,
        currentTotalBalance: previousTotalBalance,
        status: "Pending",
        title: "Deposit Request",
        description: `Requested deposit of ${amount}`,
        transactionType: "deposit",
        transactionInto: "user",
        merchantTransactionId: paymentResponse.data.merchantTransactionId,
      };

      const transaction = await transactionRepository.createTransaction(
        transactionData,
        session
      );
      console.log("Transaction created:", transaction);

      // Commit the transaction
      await session.commitTransaction();
      session.endSession();

      return paymentResponse;
    } catch (error) {
      console.error("Error during deposit handling:", error);
      await session.abortTransaction();
      session.endSession();
      throw { statusCode: error.statusCode || 500, message: error.message };
    }
  }

  //Active Function
  async handleDepositUpiIntent(userId, amount, platform, targetApp) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      const user = await userRepository.findById(userId);
      if (!user) {
        console.error("User not found");
        throw { statusCode: 404, message: "User not found" };
      }

      // Initiate PhonePe Payment
      const paymentResponse = await initiatePhonePeMobileSDK(user, amount);

      const transSetData = await TransactionSettings.findOne({});

      //GST calc
      const factor = 1 + (transSetData.sgst + transSetData.cgst) / 100;
      const previousTotalBalance = user.totalBalance;
      const baseAmount = (amount / factor).toFixed(2);

      // Calculate GST
      const gstPercentage = transSetData.sgst + transSetData.cgst; // 28% GST
      const sgstAmount = baseAmount * (transSetData.sgst / 100).toFixed(2);
      const cgstAmount = baseAmount * (transSetData.cgst / 100).toFixed(2);

      const totalGstAmount = sgstAmount + cgstAmount;

      // Create a new transaction
      const transactionData = {
        userId: user._id,
        amount: amount,
        previousTotalBalance: previousTotalBalance,
        currentTotalBalance: previousTotalBalance,
        status: "Pending",
        title: "Deposit Request",
        description: `Requested deposit of ${amount}`,
        transactionType: "deposit",
        transactionInto: "user",
        merchantTransactionId: paymentResponse.transactionId,
        gstDetails: {
          igstPercentage: transSetData.cgst,
          sgstPercentage: transSetData.sgst,
          totalGstPercentage: gstPercentage,
          igstAmount: sgstAmount,
          cgstAmount: cgstAmount,
          totalGstAmount: totalGstAmount,
        },
        netAmount: amount - totalGstAmount,
      };

      const transaction = await transactionRepository.createTransaction(
        transactionData,
        session
      );

      // Commit the transaction
      await session.commitTransaction();
      session.endSession();
      return paymentResponse;
    } catch (error) {
      await session.abortTransaction();
      session.endSession();
      throw error;
    }
  }

  async handleDepositTest(userId, amount) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      console.log(
        `Starting deposit for userId: ${userId} with amount: ${amount}`
      );
      const user = await userRepository.findById(userId);
      if (!user) {
        console.error("User not found");
        throw { statusCode: 404, message: "User not found" };
      }

      const previousTotalBalance = user.totalBalance; // Ensure previousTotalBalance is defined

      // Initiate PhonePe Payment
      const paymentResponse = await initiatePhonePePaymentTest(user, amount);
      console.log("Payment response:", paymentResponse);
      if (!paymentResponse.success) {
        console.error("Payment initiation failed");
        throw { statusCode: 400, message: "Payment initiation failed" };
      }

      // Create a new transaction
      const transactionData = {
        userId: user._id,
        amount: amount,
        previousTotalBalance: previousTotalBalance,
        currentTotalBalance: previousTotalBalance,
        status: "Pending",
        title: "Deposit Request",
        description: `Requested deposit of ${amount}`,
        transactionType: "deposit",
        transactionInto: "user",
        merchantTransactionId: paymentResponse.data.merchantTransactionId,
      };

      const transaction = await transactionRepository.createTransaction(
        transactionData,
        session
      );
      console.log("Transaction created:", transaction);

      // Commit the transaction
      await session.commitTransaction();
      session.endSession();

      return paymentResponse;
    } catch (error) {
      console.error("Error during deposit handling:", error);
      await session.abortTransaction();
      session.endSession();
      throw { statusCode: error.statusCode || 500, message: error.message };
    }
  }

  async processCallback(base64String, XVerify) {
    try {
      // Verify callback request
      if (!verifyCallbackRequest(XVerify, base64String)) {
        console.error("Security Alert : Responce Mismatched");
        throw {
          message: "Security Alert : Responce Mismatched",
        };
      }
      const decodedBuffer = Buffer.from(base64String, "base64");
      const decodedString = decodedBuffer.toString("utf8");
      const decodedObject = JSON.parse(decodedString);

      await this.updateTransactionData(decodedObject);

      console.log("Callback Processed : ");
      return { message: "Callback processed" };
    } catch (error) {
      throw { message: `Internal server error : ${error.message}` };
    }
  }

  async updateTransactionData(phonePeResponce, tID) {
    const { code } = phonePeResponce;

    const { paymentInstrument, merchantTransactionId, transactionId } =
      phonePeResponce.data;

    try {
      if (
        code == "PAYMENT_SUCCESS" ||
        code == "PAYMENT_ERROR" ||
        code == "PAYMENT_PENDING" ||
        code == "PAYMENT_DECLINED" ||
        code == "TIMED_OUT" ||
        code == "TRANSACTION_NOT_FOUND"
      ) {
        // Find transaction
        const transaction = await transactionRepository.findByPeTransactionId(
          merchantTransactionId ?? tID
        );

        if (!transaction) {
          console.error("Transaction not found");
          throw { statusCode: 404, message: "Transaction Not Found" };
        }

        //Success Transaction
        if (code == "PAYMENT_SUCCESS") {
          transaction.status = "Success";
          transaction.transactionId = transactionId;
          transaction.paymentInstrument = paymentInstrument;
          transaction.currentTotalBalance += transaction.amount;
          await transactionRepository.updateTransaction(transaction);

          //Update Balance
          const user = await userRepository.findById(transaction.userId);

          //Add net Amount to User's Balance
          user.depositBalance += transaction.netAmount;

          //Add GST to Bonus
          user.cashBonus += transaction.gstDetails.totalGstAmount;

          await userRepository.updateUser(user);
          return transaction;
        }
        //Failed Transaction
        else if (
          code == "PAYMENT_ERROR" ||
          code == "PAYMENT_DECLINED" ||
          code == "TIMED_OUT"
        ) {
          transaction.status = "Failed";
          await transactionRepository.updateTransaction(transaction);
          return transaction;
        } else if (code == "TRANSACTION_NOT_FOUND") {
          transaction.status = "Invalid";
          await transactionRepository.updateTransaction(transaction);
          return transaction;
        }

        //Pending transaction
        else {
          throw { statusCode: 500, message: "Transaction Pending" };
        }
      } else {
        throw {
          statusCode: 500,
          message: `Invalid Transaction Status : ${code}`,
        };
      }
    } catch (error) {
      console.log("Error Updating Transaction ", error);
      throw error;
    }
  }
  async updateCashfreeTransactionStatus(status,order_id) {
    try {
      const status = await getCashfreeTransactionStatus(merchantTransactionId);
      return status;
    } catch (error) {
      throw { statusCode: 500, message: error.message };
    }
  } 

  async checkTransactionStatus(merchantTransactionId) {
    try {
      const status = await getTransactionStatus(merchantTransactionId);
      return status;
    } catch (error) {
      throw { statusCode: 500, message: error.message };
    }
  }
  async getTransactionStatus(merchantTransactionId) {
    const transaction = await transactionRepository.findByPeTransactionId(
      merchantTransactionId
    );

    if (
      (transaction && transaction.status == "Success") ||
      transaction.status == "Failed" ||
      transaction.status == "Invalid"
    ) {
      return transaction;
    } else {
      return null;
    }
  }

  async getTrans(userId) {
    try {
      const transactions = await transactionRepository.findTransById(userId);
      return transactions;
    } catch (error) {
      console.error("Error fetching Transactions", error);
      throw error;
    }
  }

  async getAllTrans() {
    return await transactionRepository.findAllTrans();
  }

  async manualDeposit(userId, amount, screenshot) {
    return await transactionRepository.requestDeposit(
      userId,
      amount,
      screenshot
    );
  }

  async manualWithdrawal(userId, amount, upiId) {
    return await transactionRepository.requestWithdrawal(userId, amount, upiId);
  }

  async approveDeposit(userId, transactionId) {
    return await transactionRepository.approveDeposit(userId, transactionId);
  }

  async approveWithdrawal(userId, transactionId) {
    return await transactionRepository.approveWithdrawal(userId, transactionId);
  }

  async approvedDeposits() {
    return await transactionRepository.getApprovedDeposit();
  }

  async pendingDeposits() {
    return await transactionRepository.getPendingDeposit();
  }

  async createRazorpayOrder(userId, amount) {
    const session = await mongoose.startSession();
    session.startTransaction();

    try {
      console.log(
        `Starting deposit for userId: ${userId} with amount: ${amount}`
      );
      const user = await userRepository.findById(userId);
      if (!user) {
        console.error("User not found");
        throw { statusCode: 404, message: "User not found" };
      }
      const previousTotalBalance = user.totalBalance; // Ensure previousTotalBalance is defined
      // Initiate PhonePe Payment
      const paymentResponse = await createOrderWithRazorpay(user, amount);
      console.log("Payment response:", paymentResponse);
      if (!paymentResponse) {
        console.error("Payment initiation failed");
        throw { statusCode: 400, message: "Payment initiation failed" };
      }

      // Create a new transaction
      const transactionData = {
        userId: user._id,
        amount: amount,
        previousTotalBalance: previousTotalBalance,
        currentTotalBalance: previousTotalBalance,
        status: "Pending",
        title: "Deposit Request",
        description: `Requested deposit of ${amount}`,
        transactionType: "deposit",
        transactionInto: "user",
        merchantTransactionId: paymentResponse.data.merchantTransactionId,
      };

      const transaction = await transactionRepository.createTransaction(
        transactionData,
        session
      );
      console.log("Transaction created:", transaction);

      // Commit the transaction
      await session.commitTransaction();
      session.endSession();

      return paymentResponse;
      //   await transactionRepository.createRazorpayOrder(amount);
    } catch (error) {
      await session.abortTransaction();
      session.endSession();
      throw error;
    }
  }
}

module.exports = new transactionService();
