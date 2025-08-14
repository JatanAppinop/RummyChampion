const axios = require("axios");
const PayProfile = require("../models/PayProfile");
const FundAccount = require("../models/FundAccount");
require("dotenv").config();
const contactUrl = "https://sandbox.cashfree.com/payout/beneficiary";
const payOutUrl = "https://payout-api.cashfree.com/payout/requestTransfer";

const headers = {
  "Content-Type": "application/json",
  "x-api-version": "2024-01-01",
  "x-client-id": process.env.CASHFREE_CLIENT_ID_DEV,
  "x-client-secret": process.env.CASHFREE_SECRET_DEV,
};

async function createContact(
  username,
  mobileNumber,
  userId,
  ifsc,
  accountNumber,
  accountType,
  vpa
) {
  try {
    const existingContact = await PayProfile.findOne({ userId });

    if (existingContact) {
      return existingContact;
    }
    const requestData = {
      beneficiary_id: userId,
      beneficiary_name: username,
      beneficiary_instrument_details: {
        bank_account_number: accountNumber,
        bank_ifsc: ifsc,
        vpa: vpa,
      },
      beneficiary_contact_details: {
        beneficiary_phone: mobileNumber,
      },
    };
    const response = await axios.post(contactUrl, requestData, { headers });

    const contactData = response.data;
    const newFundAccount = new FundAccount({
      userId: userId,
      fundAccountId: contactData.id,
      entity: "beneficiary",
      contactId: contactData.id,
      accountType: accountType,
      bankAccount:
        accountType === "bank_account"
          ? { ifsc, name: username, account_number: accountNumber }
          : null,
      vpa: accountType === "vpa" ? { address: vpa } : null,
      active: true,
    });
    await newFundAccount.save();

    const payProfile = new PayProfile({
      contactId: contactData.id,
      name: contactData.beneficiary_name,
      active: true,
      userId: contactData.beneficiary_id,
      contact: contactData.beneficiary_contact_details.beneficiary_phone,
    });
    await payProfile.save();

    return contactData;
  } catch (error) {
    console.log("🚀 ~ createContact ~ error:", error);
    throw error;
  }
}

async function createFundAccount(
  contactId,
  accountType,
  username,
  ifsc,
  accountNumber,
  vpa,
  userId
) {
  try {
    let payload = {
      beneId: contactId,
      name: username,
      // email: `${userId}@example.com`,
      // phone: "",
      // address1: "",
      // city: "",
      // state: "",
      // pincode: "000000"
    };

    if (accountType === "bank_account") {
      payload.bankAccount = accountNumber;
      payload.ifsc = ifsc;
    } else if (accountType === "vpa") {
      payload.vpa = vpa;
    } else {
      throw new Error("Invalid account type");
    }

    const response = await axios.post(contactUrl, payload, { headers });
    const fundAccountData = response.data;

    const newFundAccount = new FundAccount({
      userId: userId,
      fundAccountId: contactId,
      entity: "beneficiary",
      contactId: contactId,
      accountType: accountType,
      bankAccount:
        accountType === "bank_account"
          ? { ifsc, name: username, account_number: accountNumber }
          : null,
      vpa: accountType === "vpa" ? { address: vpa } : null,
      active: true,
    });
    await newFundAccount.save();

    return fundAccountData;
  } catch (error) {
    console.error(
      "Error creating fund account:",
      error.response?.data || error.message
    );
    throw error;
  }
}

async function createPayout({ amount, fundAccountId }) {
  try {
    const payload = {
      beneId: fundAccountId,
      amount: amount,
      transferId: `payout_${Date.now()}`,
      transferMode: "imps",
      remarks: "payout",
    };

    const response = await axios.post(payOutUrl, payload, { headers });
    return response.data;
  } catch (error) {
    console.error(
      "Error creating payout:",
      error.response?.data || error.message
    );
    throw new Error(error.response?.data?.message || "Failed to create payout");
  }
}

module.exports = {
  createPayout,
  createContact,
  createFundAccount,
};
