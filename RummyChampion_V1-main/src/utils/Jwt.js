const jwt = require("jsonwebtoken");

module.exports = {
    generateToken: async (data, secret, expiryTime) => {
        const token = await jwt.sign({
            data: data,
        }, secret, {
            expiresIn: expiryTime
        });
        return token;
    },
    verifyToken: async (token, secret) => {
        return await jwt.verify(token, secret);
    },
};
