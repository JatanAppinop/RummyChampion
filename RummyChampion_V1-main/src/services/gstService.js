const gstRepository = require('../repositories/gstRepository');


class gstService {
    async addGSTInvoice(invoiceData) {
        return await gstRepository.createGSTInvoice(invoiceData);
    }

    async fetchGSTInvoices(filter, options) {
        return await gstRepository.getGSTInvoices(filter, options);
    }

    async fetchGSTInvoiceById(id) {
        return await gstRepository.getGSTInvoiceById(id);
    }

}

module.exports = new gstService()
