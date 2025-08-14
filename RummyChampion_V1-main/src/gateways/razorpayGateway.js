const Razorpay = require("razorpay");
const axios = require("axios");

const razorpay = new Razorpay({
  key_id: "rzp_test_ZekAI389hMXK6Q",
  key_secret: "Wj0pU0XS5e9T9Wq0kA4JQSLM",
});
async function createOrderWithRazorpay(customerDetails, amount) {
  try {
    // Payload for creating a Payment Link
    const payload = {
      amount: amount * 100, // Amount in paisa (Razorpay uses the smallest currency unit)
      expire_by: Math.floor(new Date().getTime() + 19800000 / 1000) + 15 * 60,
      currency: "INR",
      accept_partial: false, // Set true to allow partial payments
      description: `Payment for Order ${Date.now()}`,
      customer: {
        name: customerDetails.username,
        contact: customerDetails.mobileNumber, // Customer phone number
        // email: customerDetails.email, // Customer email address
      },
      // notify: {
      //     sms: true,
      //     email: true
      // },
      // reminder_enable: true,
      reference_id: `payin_${Date.now()}`,
      callback_url: "https://yourcallbackurl.com",
      callback_method: "get",
    };

    console.log(payload, "Creating Payment Link...");

    // Making API call to Razorpay
    const response = await razorpay.paymentLink.create(
      //   `https://api.razorpay.com/v1/payment_links`,
      payload
      //   {
      //     headers: {
      //       Authorization: `Basic ${Buffer.from(
      //         `${razorpay.key_id}:${razorpay.key_secret}`
      //       ).toString("base64")}`,
      //       "Content-Type": "application/json",
      //     },
      //   }
    );

    console.log(response, "Payment Link Created Successfully!");
    return response;
  } catch (error) {
    console.error("Error creating pay-in:", error);
    throw new Error(error.response || "Failed to create pay-in");
  }
}

async function getTransactionStatus(transactionId) {
  try {
    const hash = sha256(`${razorpay.key_secret}`) + "###1";
    const response = await axios.paymentLink.fetch(transactionId, {
      headers: {
        "Content-Type": "application/json",
        "X-VERIFY": hash,
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

async function getAllTransactions() {
  try {
    const response = await razorpay.paymentLink.all();
    return response.data;
  } catch (error) {
    console.error("Error fetching all transactions:", error);
    throw new Error(
      error.response?.data?.message ||
        error.message ||
        "Failed to fetch all transactions"
    );
  }
}
module.exports = { createOrderWithRazorpay, getTransactionStatus };
