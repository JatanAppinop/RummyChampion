const joi = require("joi");
const loginvalidation = async (req, res, next) => {
    try {
        const loginSchema = joi.object({
            email: joi.string().email().required(),
            password: joi.string().required(),
        });
        //.or("Email", "MobileNo");
        const data = req.body;
        await loginSchema.validateAsync(data, {
            allowUnknown: true,
            errors: {
                wrap: {
                    label: "",
                },
            },
        });
    } catch (err) {
        return res.status(422).json({
            success: false,
            message: err.message,
            data: [],
        });
    }

    next();
};

const passwordvalidation = async (req, res, next) => {
    try {
        const passwordSchema = joi.object({
            email: joi.string().email().required(),
            oldPassword: joi
                .string()
                .regex(/^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$/)
                .required()
                //.label("Old password")
                .messages({
                    "string.pattern.base":
                        " Old password Must have at least 8 characters and one Uppercase one lowercase and special charachter and number",
                }),
            newPassword: joi
                .string()
                .regex(/^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$/)
                .required()
                .messages({
                    "string.pattern.base":
                        "New password Must have at least 8 characters and one Uppercase one lowercase and a special charachter and number",
                }),
            // cpassword: joi
            //     .any()
            //     .valid(joi.ref("newPassword"))
            //     .required()
            //     .label("Cpassword")
            //     .options({
            //         messages: {
            //             "any.only": "{{#label}} does not match with new password!",
            //         },
            //     }),
        });
        const data = req.body;
        await passwordSchema.validateAsync(data, {
            allowUnknown: true,
            errors: {
                wrap: {
                    label: "",
                },
            },
        });
    } catch (err) {
        return res.status(422).json({
            success: false,
            message: err.message,
            data: [],
        });
    }

    next();
};

module.exports = { loginvalidation, passwordvalidation };
