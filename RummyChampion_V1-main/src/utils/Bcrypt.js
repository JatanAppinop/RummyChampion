const bcrypt = require("bcrypt");

module.exports = {
    passwordEncryption(password) {
        return new Promise((resolve, reject) => {
            bcrypt.hash(password, 10, (err, hash) => {
                resolve(hash)
            })
        })
    },
    passwordComparison:(enteredPassword, dbPassword) => {
        const promise = new Promise((resolve, reject) => {
            bcrypt.compare(enteredPassword, dbPassword, (err, same) => {
                if(err) reject(err)
                resolve(same);
            });
        });
        return promise;
    },
    passwordComparision(enteredPassword,servicePassword) {
        return new Promise((resolve, reject) => {
            bcrypt.compare(enteredPassword, servicePassword, (err, same) => {
                resolve(same)
            })
        })
    }
}