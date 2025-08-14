const { ApiCallPost } = require('./ApiCall.js');
const errorHandler  = require('./CustomError.js');
const KYC = require('../models/Kyc');
const { CASHFREE_CLIENT_ID_DEV, CASHFREE_SECRET_DEV, CASHFREE_CLIENT_ID_PROD, CASHFREE_SECRET_PROD } = process.env;

const env = "production" // production /  development
const CASHFREE_CLIENT_ID = env === "development" ? CASHFREE_CLIENT_ID_DEV : CASHFREE_CLIENT_ID_PROD;
const CASHFREE_SECRET = env === "development" ? CASHFREE_SECRET_DEV : CASHFREE_SECRET_PROD;


const fetchAadharOtp = async (aadharNumber) => {
    try {
        const devUrl = 'https://sandbox.cashfree.com/verification/offline-aadhaar/otp';
        const prodUrl = 'https://api.cashfree.com/verification/offline-aadhaar/otp';
        const URL = env === "development" ? devUrl : prodUrl;
        const headers = {
            accept: 'application/json',
            'content-type': 'application/json',
            'x-client-id': CASHFREE_CLIENT_ID,
            'x-client-secret': CASHFREE_SECRET,
        };
        const payload = { aadhaar_number: aadharNumber }

        const response = await ApiCallPost(URL, payload, headers);
        return response

    } catch (error) {
        console.log(error)
        throw await errorHandler(error.message, 500);
    }
};

const validateAadharOtp = async (otp, refId) => {
    try {
        const devUrl = 'https://sandbox.cashfree.com/verification/offline-aadhaar/verify';
        const prodUrl = 'https://api.cashfree.com/verification/offline-aadhaar/verify';
        const URL = env === "development" ? devUrl : prodUrl;
        const headers = {
            accept: 'application/json',
            'content-type': 'application/json',
            'x-client-id': CASHFREE_CLIENT_ID,
            'x-client-secret': CASHFREE_SECRET,
        };
        const payload = { otp: otp, ref_id: refId }
        const response = await ApiCallPost(URL, payload, headers);
        return response

    } catch (error) {
        throw await errorHandler(error.message, 500);
    }
};

const validatePAN = async (pan, name) => {
    try {
        const devUrl = 'https://sandbox.cashfree.com/verification/pan';
        const prodUrl = 'https://api.cashfree.com/verification/pan';
        const URL = env === "development" ? devUrl : prodUrl;
        const headers = {
            accept: 'application/json',
            'content-type': 'application/json',
            'x-client-id': CASHFREE_CLIENT_ID,
            'x-client-secret': CASHFREE_SECRET,
        };
        const payload = { pan: pan, name: name }

        const response = await ApiCallPost(URL, payload, headers);
        console.log(response);
        return response

    } catch (error) {
        throw await errorHandler(error.message, 422);
    }
};

const validateBank = async (bank_account, ifsc) => {
    try {
        const devUrl = 'https://sandbox.cashfree.com/verification/bank-account/sync';
        const prodUrl = 'https://api.cashfree.com/verification/bank-account/sync';
        const URL = env === "development" ? devUrl : prodUrl;
        const headers = {
            accept: 'application/json',
            'content-type': 'application/json',
            'x-client-id': CASHFREE_CLIENT_ID,
            'x-client-secret': CASHFREE_SECRET,
        };
        const payload = { bank_account, ifsc }

        const response = await ApiCallPost(URL, payload, headers);
        console.log(response);

        return response

    } catch (error) {
        throw await errorHandler(error.stack, 500);
    }
};

const validateUpi = async (vpa, name) => {
    try {
        const devUrl = 'https://sandbox.cashfree.com/verification/upi/advance';
        const prodUrl = 'https://api.cashfree.com/verification/upi/advance';
        const URL = env === "development" ? devUrl : prodUrl;
        const headers = {
            accept: 'application/json',
            'content-type': 'application/json',
            'x-client-id': CASHFREE_CLIENT_ID,
            'x-client-secret': CASHFREE_SECRET,
        };
        const payload = { vpa, name }

        const response = await ApiCallPost(URL, payload, headers);
        return response

    } catch (error) {
        throw await errorHandler(error.message, 500);
    }
};

const validateNameMatch = async (name_1, name_2, verification_id) => {
    try {
        const devUrl = 'https://sandbox.cashfree.com/verification/name-match';
        const prodUrl = 'https://api.cashfree.com/verification/name-match';
        const URL = env === "development" ? devUrl : prodUrl;
        const headers = {
            accept: 'application/json',
            'content-type': 'application/json',
            'x-client-id': CASHFREE_CLIENT_ID,
            'x-client-secret': CASHFREE_SECRET,
        };
        const payload = { name_1, name_2, verification_id }

        const response = await ApiCallPost(URL, payload, headers);
        return response

    } catch (error) {
        throw await errorHandler(error.message, 500);
    }
};
const updateAadharKyc = async (userId, name, dob, address, photoLink, status) => {
    return KYC.findOneAndUpdate(
        { userId },
        {
            $set: {
                "aadharKyc.nameOnAadhar": name,
                "aadharKyc.dob": dob,
                "aadharKyc.aadharAddress": address,
                "aadharKyc.imageOnAadhar": photoLink,
                "aadharKyc.aadharStatus": status,
            },
        },
        { new: true, upsert: true }
    );
};

const updateAadharKycStatusInUserProfile = async (userId, status, address, name, dob) => {
    return User.findOneAndUpdate(
        { _id: userId },
        {
            $set: {
                "kycStatus.aadharStatus": status,
                "profile.name": name,
                "profile.dob": dob,
                "profile.address": address,
            },
        },
        { new: true }
    );
};

const updatePanKyc = async (userId, name, panNumber, status) => {
    return KYC.findOneAndUpdate(
        { userId },
        {
            $set: {
                "panKyc.nameOnPan": name,
                "panKyc.panNumber": panNumber,
                "panKyc.panStatus": status,
            },
        },
        { new: true, upsert: true }
    );
};

const updatePanKycStatusInUserProfile = async (userId, status) => {
    return User.findOneAndUpdate(
        { _id: userId },
        { $set: { "kycStatus.panStatus": status } },
        { new: true }
    );
};

const validatePanRequest = async (userId) => {
    const kycData = await KYC.findOne({ userId });
    if (!kycData) return 1;

    const MAX_ATTEMPTS = 5;
    const TIME_LIMIT = 24 * 60 * 60 * 1000;
    const currentTime = new Date();
    const lastAttemptTime = kycData.panKyc.lastAttemptTime || 0;
    const timeDifference = currentTime - lastAttemptTime;

    if (timeDifference > TIME_LIMIT) {
        await KYC.updateOne({ userId }, { $set: { "panKyc.attemptCount": 1, "panKyc.lastAttemptTime": currentTime } });
        return 1;
    }
    
    if (kycData.panKyc.attemptCount >= MAX_ATTEMPTS) {
        throw errorHandler("You have reached the max attempt. Try after 24 hours", 200);
    }

    await KYC.updateOne(
        { userId },
        { $set: { "panKyc.lastAttemptTime": currentTime }, $inc: { "panKyc.attemptCount": 1 } }
    );

    return kycData.panKyc.attemptCount + 1;
};

const ApplicableToVerifyPanKyc = async (userId) => {
    const kycData = await KYC.findOne({ userId });
    if (kycData && kycData.panKyc.panStatus === "Approved") {
        throw errorHandler("PAN already verified", 400);
    }
};

const validatePanExist = async (userId, panNumber) => {
    const existingKyc = await KYC.findOne({ "panKyc.panNumber": panNumber, userId: { $ne: userId } });
    if (existingKyc) {
        throw errorHandler("This PAN is already registered with another account", 400);
    }
};


module.exports = {
    fetchAadharOtp,
    validateAadharOtp,
    validatePAN,
    validateBank,
    validateUpi,
    validateNameMatch,
    updateAadharKyc,
    updateAadharKycStatusInUserProfile,
    updatePanKyc,
    updatePanKycStatusInUserProfile,
    validatePanRequest,
    ApplicableToVerifyPanKyc,
    validatePanExist,
 };


