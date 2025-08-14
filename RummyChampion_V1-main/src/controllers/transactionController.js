const { func } = require("joi");
const transactionService = require("../services/transactionService");
const {
  createContact,
  createFundAccount,
} = require("../gateways/payoutGateway");
const TransactionSettings = require("../models/TransactionSettings");
const {
  createPaymentLink,
  getPaymentStatus,
} = require("../gateways/cashfreeGateway");
const userRepository = require("../repositories/userRepository");
const transactionRepository = require("../repositories/transactionRepository");
const mongoose = require("mongoose");

async function getBankDetail(req, res) {
  const { userId } = req.user;
  try {
    const newUser = await transactionService.bankDetail(userId);
    res.status(200).json({
      success: true,
      message: "Login Successfull",
      data: newUser,
    });
  } catch (error) {
    console.error("Error signing up:", error);
    res.status(500).json({
      success: false,
      message: "Failed to register user",
      data: [],
    });
  }
}

async function addBankDetails(req, res) {
  // const { benificaryName, accountNumber, confirmAccountNumber, mobileNumber, ifscCode } = req.body;
  const { userId } = req.user;
  const bankData = req.body;
  // { benificaryName, accountNumber, ifscCode, mobileNumber, userId }
  try {
    const newUser = await transactionService.addBank(userId, bankData);
    return res.status(200).json({
      success: true,
      message: 'Bank details added successfully"',
      data: newUser,
    });
  } catch (error) {
    return res
      .status(500)
      .json({ success: false, message: error.message, data: [] });
  }
}

async function getUserWallet(req, res) {
  const { userId } = req.user;

  try {
    const wallets = await transactionService.getWallet(userId);
    return res.status(200).json({
      success: true,
      message: "Wallet details fetched successfully",
      data: wallets,
    });
  } catch (error) {
    return res
      .status(500)
      .json({ success: false, message: error.message, data: [] });
  }
}

async function updateUserBalance(req, res) {
  const { userId, balance } = req.body;
  try {
    const wallets = await transactionService.updateWallet(userId, balance);
    return res.status(200).json({
      success: true,
      message: 'Wallet details Updated successfully"',
      data: wallets,
    });
  } catch (error) {
    return res
      .status(500)
      .json({ success: false, message: error.stack, data: [] });
  }
}

async function fundsAndPay(req, res) {
  const { accountType, ifsc, accountNumber, vpa, fullname } = req.body;
  const { userId, mobileNumber } = req.user;
  // console.log(req.user, "::::::::::");
  // console.log(req.body, "::::::::::>>>>");
  try {
    const contactResponse = await createContact(
      fullname,
      mobileNumber,
      userId,
      ifsc,
      accountNumber,
      accountType,
      vpa
    );
    // console.log(contactResponse, "????");
    // const fundAccountResponse = await createFundAccount(
    //   contactResponse.contactId,
    //   accountType,
    //   fullname,
    //   ifsc,
    //   accountNumber,
    //   vpa,
    //   userId
    // );
    res.status(200).json({
      success: true,
      contact: contactResponse,
      // fundAccount: fundAccountResponse,
    });
  } catch (error) {
    console.error("Error processing account creation:", error);

    // Send error response
    res.status(500).json({
      success: false,
      message: "Error processing account creation",
      error: error.response?.data || error.message,
    });
  }
}

async function requestWithdrawal(req, res) {
  try {
    const { userId } = req.user;
    const { amount, fundAccountId, accountType } = req.body;

    if (!userId || !amount) {
      return res
        .status(400)
        .json({ message: "User ID and amount are required" });
    }

    const data = await transactionService.requestWithdrawal(
      userId,
      amount,
      fundAccountId,
      accountType
    );
    res.status(201).json({
      success: true,
      message: "Withdrawal request submitted successfully",
      data,
    });
  } catch (error) {
    res
      .status(error.statusCode || 500)
      .json({ success: false, message: error.message });
  }
}

async function handleWithdrawalRequest(req, res) {
  try {
    const { requestId, status } = req.body;
    if (!requestId || !status) {
      return res
        .status(400)
        .json({ message: "Request ID and status are required" });
    }
    const result = await transactionService.handleWithdrawalRequest(
      requestId,
      status
    );
    res.status(200).json({
      success: true,
      message: "Withdrawal request handled successfully",
      data: result,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function pendingTransaction(req, res) {
  try {
    const trans = await transactionService.getTransByStatus("Pending");
    return res.status(200).json({
      success: true,
      message: "Pending Transactions fetched successfully",
      trans,
    });
  } catch (error) {
    return res
      .status(500)
      .json({ success: false, message: error.message, data: [] });
  }
}

async function approvedTransaction(req, res) {
  try {
    const trans = await transactionService.getTransByStatus("Approved");
    return res.status(200).json({
      success: true,
      message: "Approved Transactions fetched successfully",
      data: trans,
    });
  } catch (error) {
    return res
      .status(500)
      .json({ success: false, message: error.message, data: [] });
  }
}

async function rejectedTransaction(req, res) {
  try {
    const trans = await transactionService.getTransByStatus("Rejected");
    return res.status(200).json({
      success: true,
      message: "Rejected Transactions fetched successfully",
      trans,
    });
  } catch (error) {
    return res
      .status(500)
      .json({ success: false, message: error.message, data: [] });
  }
}

async function requestDeposit(req, res) {
  const { amount } = req.body;
  const { userId } = req.user;

  if (amount === undefined) {
    return res.status(400).json({
      success: false,
      message: "amount is required.",
    });
  }

  try {
    const result = await transactionService.handleDepositUpiIntent(
      userId,
      amount
    );

    return res.status(200).json({
      success: true,
      message: "Base Data Generated for PhonePE Mobile SDK",
      data: result,
    });
  } catch (error) {
    return res.status(500).json({
      success: false,
      message: error.message,
      data: [],
      ...error,
    });
  }
}

//Dont Use for Live API
async function requestDepositTest(req, res) {
  const { amount } = req.body;
  const { userId } = req.user;

  if (amount === undefined) {
    return res.status(400).json({
      success: false,
      message: "amount is required.",
    });
  }

  try {
    const result = await transactionService.handleDepositUpiIntent(
      userId,
      amount
    );

    return res.status(200).json({
      success: true,
      message: "Base Data Generated for PhonePE Mobile SDK",
      data: result,
    });
  } catch (error) {
    return res.status(500).json({
      success: false,
      message: error.stack,
      name: error.message,
      data: [],
      ...error,
    });
  }
}

async function checkTransactionStatus(req, res) {
  try {
    const { tid } = req.params;

    if (tid === undefined) {
      return res.status(400).json({
        success: false,
        message: "Transaction ID  is required.",
      });
    }

    const transaction = await transactionService.getTransactionStatus(tid);

    if (transaction) {
      console.log("Transaction alreay Update");
      return res.status(200).json({
        success: true,
        message: transaction.status,
        data: transaction,
      });
    }

    //Get Transaction Status from Phonepe
    const status = await transactionService.checkTransactionStatus(tid);

    //Update Database According to the Data
    const responce = await transactionService.updateTransactionData(
      status,
      tid
    );
    return res
      .status(200)
      .json({ success: true, message: responce.status, data: responce });
  } catch (error) {
    console.error("Error checking transaction status:", error);
    return res
      .status(error.statusCode || 500)
      .json({ success: false, message: error.message });
  }
}

async function handleCallback(req, res) {
  const xVerifyHeader = req.headers["x-verify"];
  const { response } = req.body;
  try {
    await transactionService.processCallback(response, xVerifyHeader);
    return res.status(200).json({ success: true });
  } catch (error) {
    console.log("PhonePe Callback Error : " + error.message);
    return res.status(500).json({ success: false, message: "Error" });
  }
}

async function getTransactions(req, res) {
  const { userId } = req.user;
  try {
    const data = await transactionService.getTrans(userId);
    res.status(200).json({
      success: true,
      message: `transactions data of user ${userId}`,
      data,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function getAllTransactions(req, res) {
  try {
    const data = await transactionService.getAllTrans();
    res.status(200).json({ success: true, message: `transactions data`, data });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function manualDeposit(req, res) {
  const { amount } = req.body;
  const screenshot = req.file;
  const { userId } = req.user;

  try {
    const result = await transactionService.manualDeposit(
      userId,
      amount,
      screenshot
    );
    res.status(200).json({
      success: true,
      message: "Deposit request sent to admin for approval",
      result,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function manualWithdrawal(req, res) {
  const { amount, upiId } = req.body;
  const { userId } = req.user;

  try {
    const data = await transactionService.manualWithdrawal(
      userId,
      amount,
      upiId
    );
    res.status(200).json({
      success: true,
      message: "Withdrawal request sent to admin for approval",
      data,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function approveDeposit(req, res) {
  const { userId, transactionId } = req.body;

  try {
    const result = await transactionService.approveDeposit(
      userId,
      transactionId
    );
    res.status(200).json({
      success: true,
      message: "Deposit approved successfully",
      result,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function approveWithdrawal(req, res) {
  const { userId, transactionId } = req.body;

  try {
    const result = await transactionService.approveWithdrawal(
      userId,
      transactionId
    );
    res.status(200).json({
      success: true,
      message: "Withdrawal approved successfully",
      result,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function getApprovedDeposits(req, res) {
  try {
    const data = await transactionService.approvedDeposits();
    res.status(200).json({
      success: true,
      message: "Approved Withdrawals fetched successfully",
      data,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function getPendingDeposits(req, res) {
  try {
    const data = await transactionService.pendingDeposits();
    res.status(200).json({
      success: true,
      message: "Pending Withdrawals fetched successfully",
      data,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

async function getTransactionSettings(req, res) {
  try {
    const data = await TransactionSettings.findOne({});
    res.status(200).json({
      success: true,
      message: "Transactions Settings fetched successfully",
      data,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

// Razorpay payment

async function createRazorpayOrder(req, res) {
  try {
    const data = await transactionService.createRazorpayOrder(
      req.user.userId,
      req.body.amount
    );
    console.log(data, "data");
    res.status(200).json({
      success: true,
      message: "Funds and Pay fetched successfully",
      data,
    });
  } catch (error) {
    res.status(error.statusCode || 500).json({ message: error.message });
  }
}

// Cashfree payment
async function cashFreeRequestDeposit(req, res) {
  try {
    const session = await mongoose.startSession();
    session.startTransaction();
    const { userId } = req.user;
    const { amount } = req.body;
    if (!amount) {
      return res
        .status(400)
        .json({ success: false, message: "amount is required" });
    }
    console.log(
      `Starting deposit for userId: ${userId} with amount: ${amount}`
    );
    const user = await userRepository.findById(userId);
    const previousTotalBalance = user.totalBalance; // Ensure previousTotalBalance is defined

    const wallet = await transactionService.getWallet(userId);
    if (!user) {
      return res
        .status(400)
        .json({ success: false, message: "User not found" });
    }
    const customer_details = {
      customer_id: user._id,
      customer_name: user.name,
      customer_email: user.email,
      customer_phone: user.mobileNumber,
    };
    const order_id = `ORD_${Date.now()}_${userId}`;

    const paymentData = {
      order_id,
      order_amount: +amount,
      order_currency: "INR",
      customer_details,
      order_note: "Payment for services",
      order_meta: {
        return_url: `open://rummychampions`,
      },
    };

    const paymentResponse = await createPaymentLink(paymentData);
    if (paymentResponse) {
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
        orderId: order_id,
      };
      const transaction = await transactionRepository.createTransaction(
        transactionData,
        session
      );
      console.log("Transaction created:", transaction);
    }
    // Commit the transaction
    await session.commitTransaction();
    session.endSession();
    return res.status(200).json({
      success: true,
      message: "Payment link created successfully",
      data: paymentResponse,
    });
  } catch (error) {
    return res.status(500).json({ success: false, message: error.message });
  }
}

async function checkCashFreePaymentStatus(req, res) {
  try {
    const { order_id } = req.query;

    if (!order_id) {
      return res.status(400).json({ error: "Missing order_id parameter" });
    }
    const transaction = await transactionRepository.findTransById(
      req.user.userId
    );
    if (transaction[0].status == "Approved")
      return res.status(200).json({
        success: true,
        message: "Transaction already approved",
        data: [],
      });
    if (transaction[0].status == "Failed")
      return res.status(200).json({
        success: true,
        message: "Transaction failed, please try again",
        data: [],
      });
    const paymentStatus = await getPaymentStatus(req.user.userId, order_id);
    res.json({
      success: true,
      message: "Payment status fetched successfully",
      data: paymentStatus,
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      message: "Failed to fetch payment status",
      error: error.message,
    });
  }
}

module.exports = {
  getBankDetail,
  addBankDetails,
  getUserWallet,
  updateUserBalance,
  checkTransactionStatus,
  requestWithdrawal,
  handleWithdrawalRequest,
  pendingTransaction,
  approvedTransaction,
  rejectedTransaction,
  requestDeposit,
  getTransactions,
  manualWithdrawal,
  manualDeposit,
  approveDeposit,
  approveWithdrawal,
  getApprovedDeposits,
  getPendingDeposits,
  handleCallback,
  fundsAndPay,
  requestDepositTest,
  getAllTransactions,
  getTransactionSettings,
  createRazorpayOrder,
  cashFreeRequestDeposit,
  checkCashFreePaymentStatus,
};
