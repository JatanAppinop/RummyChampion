const Joi = require('joi');

const addUserBankDetailsSchema = Joi.object({
    beneficaryName: Joi.string().required().messages({
        'any.required': 'Beneficiary name is required',
        'string.empty': 'Beneficiary name cannot be empty',
    }),
    accountNumber: Joi.string().required().messages({
        'any.required': 'Account number is required',
        'string.empty': 'Account number cannot be empty',
    }),
    confirmAccountNumber: Joi.string().valid(Joi.ref('accountNumber')).required().messages({
        'any.required': 'Confirm account number is required',
        'any.only': 'Confirm account number must match account number',
    }),
    ifscCode: Joi.string().required().messages({
        'any.required': 'IFSC code is required',
        'string.empty': 'IFSC code cannot be empty',
    }),
    mobileNumber: Joi.string().pattern(/^[0-9]{10,12}$/).required().messages({
        'any.required': 'Mobile number is required',
        'string.pattern.base': 'Mobile number must be between 10 and 12 digits',
    }),
});

module.exports = { addUserBankDetailsSchema };
