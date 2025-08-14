const gstService = require('../services/gstService');

async function addGSTInvoice(req, res) {
    try {
        const invoiceData = req.body;
        const newInvoice = await gstService.addGSTInvoice(invoiceData);
        res.status(201).json({ success: true, newInvoice });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
}

async function getGSTInvoices(req, res) {
    try {
        const { from, to, page = 1, limit = 50 } = req.query;
        const filter = {
            ...(from && { invoiceDate: { $gte: new Date(from) } }),
            ...(to && { invoiceDate: { $lte: new Date(to) } })
        };
        const options = {
            skip: (page - 1) * limit,
            limit: parseInt(limit),
            sort: { invoiceDate: -1 }
        };
        const invoices = await gstService.fetchGSTInvoices(filter, options);
        res.status(200).json({ success: true, invoices });
    } catch (error) {
        res.status(500).json({ message: error.message });
    }
}

module.exports = {
    addGSTInvoice,
    getGSTInvoices
};
