const axios = require("axios");
const  errorHandler = require("./CustomError");


const ApiCallPost = async (url, data, headers) => {
    try {
        const response = await axios.post(url, data, { headers: headers });
        return response?.data;
    } catch (error) {
        console.log(error);
        throw await errorHandler(error?.response?.data?.message, 500);
    };
};

// For Even Bet 
const ApiCallPost2 = async (url, data, headers) => {
    try {
        const response = await axios.post(url, data, { headers: headers });
        return response?.data;
    } catch (error) {
        throw await errorHandler(error?.response?.data?.errors[0]?.detail, error?.response?.data?.errors[0]?.status);
    };
};

const ApiCallGet = async (url, headers) => {
    try {
        const response = await axios.get(url, { headers: headers });
        return response?.data;
    } catch (error) {
        throw await errorHandler(error?.response?.data?.message, 500);
    };
};

module.exports = { ApiCallPost, ApiCallPost2, ApiCallGet };