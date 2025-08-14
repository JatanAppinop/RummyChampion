const axios = require("axios");
const crypto = require("crypto");
require("dotenv").config();

// const { MERCHANT_ID, MERCHANT_KEY, PHONEPE_BASE_URL, CALLBACK_URL } = process.env;
const MERCHANT_ID = process.env.MERCHANT_ID;
const MERCHANT_KEY = process.env.MERCHANT_KEY;

const CALLBACK_URL = "https://api.rummychampions.co.in/callback";
const PHONEPE_STATUS_URL = "https://api.phonepe.com/apis/hermes/pg/v1/status/";

// Helper function to generate SHA256 hash
function sha256(data) {
  return crypto.createHash("sha256").update(data).digest("hex");
}

function verifyCallbackRequest(XVerify, base64String) {
  console.log("X Verify : ", XVerify);
  console.log("string : ", base64String);
  try {
    const sha256Hash = sha256(base64String + MERCHANT_KEY) + "###" + "1";
    return XVerify === `${sha256Hash}`;
  } catch (error) {
    console.error("Error verifying callback request:", error);
    return false;
  }
}

// Generate Transaction ID , Payload and Checksum to Initiate Transaction on Mobile SDK
async function initiatePhonePeMobileSDK(user, amount) {
  try {
    const transactionId = `TXN${Date.now()}`;

    const price = parseFloat(amount);

    const payload = {
      merchantId: MERCHANT_ID,
      merchantTransactionId: transactionId,
      merchantUserId: user._id.toString(),
      amount: price * 100,
      callbackUrl: CALLBACK_URL,
      mobileNumber: user.mobileNumber.toString(),
      paymentInstrument: {
        type: "PAY_PAGE",
      },
    };

    const jsonString = JSON.stringify(payload);
    const base64Payload = Buffer.from(jsonString).toString("base64");

    const key = process.env.MERCHANT_KEY;
    const keyIndex = 1;
    const paypage = "/pg/v1/pay";
    const joinedString = base64Payload + paypage + key;
    const sha256Encoded = sha256(joinedString).toString();
    const checksum = sha256Encoded + "###" + keyIndex;

    const response = { base64Payload, checksum, transactionId };
    return response;
  } catch (error) {
    console.log("Error Fetching URL");
    console.log(error.response?.data);
    throw error.response.data || error.message || "Payment initiation failed";
  }
}

async function getTransactionStatus(merchantTransactionId) {
  try {
    const statusUrl = `${PHONEPE_STATUS_URL}${MERCHANT_ID}/${merchantTransactionId}`;
    const saltKey = process.env.MERCHANT_KEY;
    const keyIndex = 1;
    const url =
      `/pg/v1/status/${MERCHANT_ID}/${merchantTransactionId}` + saltKey;
    const sha256Encoded = sha256(url).toString();
    const XVerify = sha256Encoded + "###" + keyIndex;

    const response = await axios.get(statusUrl, {
      headers: {
        "Content-Type": "application/json",
        "X-VERIFY": XVerify,
        "X-MERCHANT-ID": MERCHANT_ID,
        accept: "application/json",
      },
    });
    return response.data;
  } catch (error) {
    console.error("Error fetching transaction status:", error);
    throw new Error(
      error.response?.data?.message ||
        error.message ||
        "Failed to fetch transaction status"
    );
  }
}

module.exports = {
  initiatePhonePeMobileSDK,
  verifyCallbackRequest,
  getTransactionStatus,
};
