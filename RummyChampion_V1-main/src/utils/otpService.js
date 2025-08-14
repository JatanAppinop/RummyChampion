const axios = require('axios');
const { AUTH_KEY, PROJECT_NAME } = process.env

function generateOTP() {
    return Math.floor(100000 + Math.random() * 900000); // Generate 6-digit OTP
}

async function sendOTPViaSMS(type, message, receiver) {
    try {
        if (type === "verification") {
            console.log('in verification')
            let sendOtp = await axios.get(
                `https://api.authkey.io/request?authkey=${AUTH_KEY}&mobile=${receiver}&country_code=+91&sid=14990&company="account at ${PROJECT_NAME}"&otp=${message}&time="1 minutes"`
            );
            // console.log("Mobile OTP send: ", sendOtp);
        } else if (type == 'exchange') {
            // We can add twilio msg service here
        }
    } catch (error) {
        console.log('we are facing some error while sending otp')
    }
};

module.exports = { generateOTP, sendOTPViaSMS };
