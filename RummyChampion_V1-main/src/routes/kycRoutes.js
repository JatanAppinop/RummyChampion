const express = require('express');
const router = express.Router();
const { submitKyc, getKycStatus, getAllKycs, approvedBank, pendingBank, rejectedBank, fundAccount,
    kycApprove, getBanks, bankApprove, approvedKyc, pendingKyc, rejectedKyc, getUserKyc, aadharOtp, verifyAadhar, submitPan
} = require('../controllers/kycController');
const upload = require('../middlewares/Upload');
const { Auth } = require('../middlewares/Auth');


router.post('/user/submitKyc', [Auth, upload.fields([{ name: "documentImage" }])], submitKyc);

router.get('/user/getsubmitkyc', [Auth], getKycStatus);

router.post('/user/aadharOtp', [Auth], aadharOtp);

router.post('/user/verifyAadhar', [Auth], verifyAadhar);

router.post('/user/submitPan', [Auth], submitPan);

router.get('/user/getFundAccount', [Auth], fundAccount)

router.get('/admin/userKyc/:userId', [Auth], getUserKyc);

router.get('/admin/getAllKycs', [Auth], getAllKycs);

router.put('/admin/kycApproval', [Auth], kycApprove);

router.get('/admin/getAllBanks', [Auth], getBanks);

router.get('/admin/kycApproved', [Auth], approvedKyc);

router.get('/admin/kycPending', [Auth], pendingKyc);

router.get('/admin/kycRejected', [Auth], rejectedKyc);

router.put('/admin/approveBank', [Auth], bankApprove);

router.get('/admin/bankApproved', [Auth], approvedBank);

router.get('/admin/bankPending', [Auth], pendingBank);

router.get('/admin/bankRejected', [Auth], rejectedBank);


module.exports = router;
