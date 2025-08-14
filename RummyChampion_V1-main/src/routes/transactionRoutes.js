const express = require("express");
const router = express.Router();
const { Auth } = require("../middlewares/Auth");
const {
  getBankDetail,
  addBankDetails,
  getUserWallet,
  updateUserBalance,
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
  checkTransactionStatus,
  cashFreeRequestDeposit,
  checkCashFreePaymentStatus
} = require("../controllers/transactionController");
const validateReq = require("../middlewares/Validate");
const upload = require("../middlewares/Upload");
const { addUserBankDetailsSchema } = require("../validations/userValidation");
const { getTransactionStatus } = require("../gateways/phonePeGateway");
const { create } = require("../models/Game");
const CashfreeGateway = require("../gateways/cashfreeGateway");

router.get("/user/userBankDetails", [Auth], getBankDetail);

router.post(
  "/user/addUserBankDetails",
  [Auth, validateReq(addUserBankDetailsSchema)],
  addBankDetails
);

router.get("/wallet/userWallet", [Auth], getUserWallet);

router.put("/admin/updateBalance", [Auth], updateUserBalance);

router.get("/admin/getPendingTransaction", [Auth], pendingTransaction);

router.get("/admin/getApprovedTransaction", [Auth], approvedTransaction);

router.get("/admin/getRejectedTransaction", [Auth], rejectedTransaction);

router.post("/user/createFunds&Pay", [Auth], fundsAndPay);

router.post("/user/requestWithdrawal", [Auth], requestWithdrawal);

router.post("/admin/handleWithdrawalRequest", [Auth], handleWithdrawalRequest);

// PhonePe
router.post("/user/requestDeposit", [Auth], requestDeposit);
router.post("/user/requestDepositTest", [Auth], requestDepositTest);
router.get("/user/checkTransaction/:tid", [Auth], checkTransactionStatus);
router.post("/callback", handleCallback);
router.get("/user/transactions", [Auth], getTransactions);

// >>>>>>>>>

// Razorpay
router.post("/user/createTransaction", createRazorpayOrder);

router.get("/admin/allTransactions", getAllTransactions);

router.post("/user/requestWithdrawalManual", [Auth], manualWithdrawal);

router.post(
  "/user/requestDepositManual",
  [Auth, upload.single("screenshot")],
  manualDeposit
);

router.get("/admin/getApprovedDeposits", [Auth], getApprovedDeposits);

router.get("/admin/getPendingDeposits", [Auth], getPendingDeposits);

router.post("/admin/approveDeposit", [Auth], approveDeposit);

router.post("/admin/approveWithdrawal", [Auth], approveWithdrawal);

router.get("/transcationSettings", getTransactionSettings);

// Razorpay
router.post("/user/createOrder", [Auth], createRazorpayOrder);

// Cash Free
router.post("/user/requestCashFree", [Auth], cashFreeRequestDeposit);
router.get("/user/cash-free-payment-status", [Auth], checkCashFreePaymentStatus);

/**
    public static string walletPage = "wallet/user-wallet";
    public static string getCoins = "coin/getCoins";
 */

module.exports = router;
