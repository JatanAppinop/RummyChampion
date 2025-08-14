const mongoose = require("mongoose")


const ApkSchema = new mongoose.Schema({
    apkfile: { type: String }
})

const APK = mongoose.model("Apks", ApkSchema);

module.exports = APK;