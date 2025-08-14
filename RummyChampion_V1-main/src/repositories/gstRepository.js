const GSTInvoice = require('../models/Gst');

class gstRepository {
    async createGSTInvoice(invoiceData) {
        const invoice = new GSTInvoice(invoiceData);
        return await invoice.save();
    }

    async getGSTInvoices(filter, options) {
        return await GSTInvoice.find(filter)
            .skip(options.skip)
            .limit(options.limit)
            .sort(options.sort);
    }

    async getGSTInvoiceById(id) {
        return await GSTInvoice.findById(id);
    }
}

module.exports = new gstRepository();