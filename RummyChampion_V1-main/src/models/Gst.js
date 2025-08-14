const mongoose = require('mongoose');
const { ObjectId } = require('bson')


const gstInvoiceSchema = new mongoose.Schema({
    invoiceNo: { type: String, required: true, unique: true },
    amount: { type: Number, required: true },
    userId: { type: ObjectId, ref: 'User', required: true },
    name: { type: String, required: true },
    email: { type: String, required: false },
    mobileNumber: { type: String, required: true },
    invoiceDate: { type: Date, required: true },
    gstAmount: { type: Number, required: true },
    stateGstAmount: { type: Number, required: true },
    createdAt: { type: Date, default: Date.now },
    updatedAt: { type: Date, default: Date.now }
});

const GST = mongoose.model('GSTInvoice', gstInvoiceSchema);

module.exports = GST
