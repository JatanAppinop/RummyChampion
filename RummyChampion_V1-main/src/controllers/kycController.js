const kycService = require('../services/kycService');
const { fetchAadharOtp, validateAadharOtp, validatePAN,    updateAadharKyc,
    updateAadharKycStatusInUserProfile,
    updatePanKyc,
    updatePanKycStatusInUserProfile,
    validatePanRequest,
    ApplicableToVerifyPanKyc,
    validatePanExist, } = require('../utils/AutomatedKyc');
const KYC = require('../models/Kyc');


async function submitKyc(req, res) {
    try {
        const { userId } = req.user;
        const files = req.files;
        const payloads = req.body;
        console.log(payloads, "...>>>>>>>");
        const data = await kycService.submitKyc(userId, files, payloads);
        return res.status(200).json({
            success: true,
            message: "KYC submitted successfully",
            data: data,
        });

    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
};

async function fundAccount(req, res) {
    try {
        const { userId } = req.user;
        const data = await kycService.getFundAccount(userId);
        return res.status(200).json({
            success: true,
            message: "Fund Accounts Fetched",
            data
        })
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message });

    }
};

async function getKycStatus(req, res) {
    try {
        const { userId } = req.user;
        console.log(userId, ">>>>>....");
        const data = await kycService.getKycStatus(userId);
        return res.status(200).json({ success: true, message: "KYC fetched successfully", data });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
};

async function getAllKycs(req, res) {
    try {
        const kyc = await kycService.getAllKyc();
        return res.status(200).json({ success: true, message: "KYC fetched successfully", kyc });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
};

async function kycApprove(req, res) {
    try {
        const { kycVerified, userId, reason } = req.body;
        const update = await kycService.updateKyc(kycVerified, userId, reason)
        return res.status(200).json({ success: true, update, message: "Status updated" })
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function getBanks(req, res) {
    try {
        const banks = await kycService.getBanks();
        return res.status(200).json({ success: true, banks, message: "Banks fetched" })
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function bankApprove(req, res) {
    try {
        const { userId, verified } = req.body;
        const update = await kycService.approveBank(userId, verified);
        return res.status(200).json({ success: true, update, message: `status set to ${verified}` })
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function approvedKyc(req, res) {
    try {
        const kyc = await kycService.getKycByStatus('Approved');
        return res.status(200).json({ success: true, message: "Approved KYC fetched successfully", kyc });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function pendingKyc(req, res) {
    try {
        const kyc = await kycService.getKycByStatus('Pending');
        return res.status(200).json({ success: true, message: "Pending KYC fetched successfully", kyc });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function rejectedKyc(req, res) {
    try {
        const kyc = await kycService.getKycByStatus('Rejected');
        return res.status(200).json({ success: true, message: "Rejected KYC fetched successfully", kyc });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function approvedBank(req, res) {
    try {
        const banks = await kycService.getBankByStatus('Approved');
        return res.status(200).json({ success: true, message: "Approved banks fetched successfully", banks });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function pendingBank(req, res) {
    try {
        const banks = await kycService.getBankByStatus('Pending');
        return res.status(200).json({ success: true, message: "Pending banks fetched successfully", banks });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function rejectedBank(req, res) {
    try {
        const banks = await kycService.getBankByStatus('Rejected');
        return res.status(200).json({ success: true, message: "Rejected banks fetched successfully", banks });
    } catch (error) {
        return res.status(500).json({ success: false, message: error.message, data: [] });
    }
}

async function getUserKyc(req, res) {
    try {
        const userId = req.params.userId;
        const userKyc = await kycService.getUserKyc(userId);
        if (!userKyc) {
            return res.status(404).json({ success: true, message: 'User KYC not found' });
        }
        res.json({ success: true, userKyc });
    } catch (error) {
        console.error('Error fetching user KYC:', error);
        res.status(500).json({ success: false, message: 'Server error' });
    }
}

async function aadharOtp(req, res) {
    let attemptedCount;
    try {
        const { userId } = req.user;
        const { aadharNumber } = req.body;
        const kycData = await KYC.findOne({ userId: userId })
        if (kycData) {
            if (kycData.aadharKyc.aadharStatus === "Approved") {
            return res.status(200).json({ success: false, message: "Aadhaar already verified" });     }
        }
        const kycData2 = await KYC.findOne({ userId: { $ne: userId }, 'aadharKyc.aadharNumber': aadharNumber })
        if (kycData2){
            return res.status(200).json({ success: false, message: "Aadhaar number already exists" });
        }
        const otp = await fetchAadharOtp(aadharNumber);
        return res.status(200).json({ success: true, message: "OTP sent successfully", otp });
    } catch (error) {
        return res.status(200).json({ success: false, message: error?.message || "Some error occurred while sending otp ", attemptedCount });
    }
}

async function verifyAadhar(req, res) {
    let attemptedCount
    try {
        const { userId } = req.user;
        const { aadharOtp, refId } = req.body;
        if (!aadharOtp) return res.status(200).json({ success: false, message: "Please enter valid otp" });
        if (!refId) return res.status(200).json({ success: false, message: "Please enter valid ref id" });

        const response = await validateAadharOtp(aadharOtp, refId);
      // **Fix: Ensure validateAadharRequest is awaited**
            attemptedCount = await KYC.findOne({ userId }).then((kyc) => {
            return kyc ? kyc.validateAadharRequest(userId) : 1;
            });    
        if (attemptedCount >= 5) {
            return res.status(404).json({ success: false, message: "You have reached the max attempt. Please try after 1hrs" });
        }
        if (response && response?.message === 'Aadhaar Card Exists') {
            let kycUpdated = await updateAadharKyc(userId, response.name, response.dob, response.address, response.photo_link, "Pending");
            await updateAadharKycStatusInUserProfile(userId, "Pending", response.address, response.name, response.dob)
            return res.status(200).json({ success: true, message: "Aadhaar verified successfully ", attemptedCount, data: response });
        }
        else {
            return res.status(200).json({ success: false, message: response?.message, attemptedCount });
        }
    } catch (error) {
        return res.status(200).json({ success: false, message: error?.message || "Some error occurred while verifying otp ", attemptedCount });
    }
}

async function submitPan(req, res) {
    let attemptedCount;
    try {
        const { userId } = req.user;
        const { panNumber } = req.body;

        await ApplicableToVerifyPanKyc(userId);
        await validatePanExist(userId, panNumber);
        const kycData = await KYC.findOne({ userId: userId })
        if (!kycData) {
            return res.status(404).json({ success: false, message: 'Please submit your Aadhaar first' });
        }
        const response = await validatePAN(panNumber, kycData.aadharKyc.nameOnPan);
        attemptedCount = await validatePanRequest(userId);
        if (response && response?.message === 'PAN verified successfully') {
            let kycUpdated = await updatePanKyc(userId, response.registered_name, panNumber, "Pending");
            await updatePanKycStatusInUserProfile(userId, "Pending");
            return res.status(200).json({ success: true, message: "PAN verified successfully ", attemptedCount, data: { registeredName: response?.registered_name, valid: response?.valid, pan: response?.pan, } });
        }
        else {
            return res.status(200).json({ success: false, message: "Invalid PAN", attemptedCount });
        }
    } catch (error) {
        return res.status(200).json({ success: false, message: error?.message || "Some error occurred while verifying PAN ", attemptedCount });
    }
}

module.exports = {
    getKycStatus, submitKyc, getAllKycs, kycApprove, getBanks,
    bankApprove, approvedKyc, pendingKyc, rejectedKyc, approvedBank,
    pendingBank, rejectedBank, getUserKyc, fundAccount, aadharOtp, verifyAadhar, submitPan
}
