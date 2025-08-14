const axios = require("axios");
const transactionService = require("../services/transactionService");
const transactionRepository = require("../repositories/transactionRepository");
require("dotenv").config();

const BASE_URL =
  process.env.CASHFREE_ENV === "sandbox"
    ? "https://api.cashfree.com"
    : "https://sandbox.cashfree.com";

const headers = {
  "Content-Type": "application/json",
  "x-client-id": process.env.CASHFREE_CLIENT_ID_DEV,
  "x-client-secret": process.env.CASHFREE_SECRET_DEV,
  "x-api-version": "2022-01-01",
};

/**
 * Create a Payment Link (Payin)
 */
const createPaymentLink = async (data) => {
  try {
    const response = await axios.post(`${BASE_URL}/pg/orders`, data, {
      headers,
    });
    console.log("🚀 ~ createPaymentLink ~ response:", response);

    return response.data;
  } catch (error) {
    console.error("Cashfree API Error:", error);
    throw new Error("Failed to create payment link");
  }
};

/**
 * Get Payment Status from Cashfree
 */
const getPaymentStatus = async (userId, orderId) => {
  try {
    const transactions = await transactionRepository.findTransById(userId);
    console.log("🚀 ~ getPaymentStatus ~ transactions:", transactions);

    const response = await axios.get(`${BASE_URL}/pg/orders/${orderId}`, { headers });
    const { order_status, order_amount } = response.data;

    const statusMapping = {
      PAID: "Approved",
      FAILED: "Failed",
      PENDING: "Pending",
      CANCELLED: "Cancelled"
    };

    transactions[0].status = statusMapping[order_status] || transactions[0].status;

    if (order_status === "PAID") {
      await transactionService.updateUserWallet(userId, order_amount);
    }

    await transactions[0].save();
    return response.data;
  } catch (error) {
    console.error("Cashfree API Error:", error.response?.data || error.message);
    throw new Error("Failed to fetch payment status");
  }
};

module.exports = {
  createPaymentLink,
  getPaymentStatus,
};
