const express = require('express');
const router = express.Router();
const { addGSTInvoice, getGSTInvoices } = require('../controllers/gstController');
const { Auth } = require('../middlewares/Auth');


router.post('/admin/gst/addInvoice', [Auth], addGSTInvoice);

router.get('/admin/gst/invoices', [Auth], getGSTInvoices);

module.exports = router;
