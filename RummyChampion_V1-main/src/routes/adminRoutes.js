const express = require("express");
const router = express.Router();
const { Auth } = require("../middlewares/Auth")
const { login, changePass, editProfile, forgotPass, getAllUsers,
    blockUser, blockAllUser, dashboard, uploads
} = require("../controllers/adminController");
const {
    loginvalidation,
    passwordvalidation,
} = require("../utils/AuthLoginValidator");
const { uploadapk } = require("../utils/ApkUpload");

router.post("/admin/login", [loginvalidation], login);

router.get('/admin/dashboard', [Auth], dashboard)

router.post("/admin/changePassword", [passwordvalidation], [Auth], changePass);

router.post("/admin/forgotPassword", [passwordvalidation], [Auth], forgotPass);

router.put("/admin/editProfile", [Auth], editProfile);

router.get("/admin/getAllUsers", [Auth], getAllUsers);

router.put("/admin/blockUser", [Auth], blockUser);

router.put("/admin/blockAllUser", [Auth], blockAllUser);

router.post("/updloadapk", [uploadapk.single("RC")], uploads)

router.get('/downloads', function (req, res, next) {
    let filePath = "public/uploads/RC.apk"; // Or format the path using the `id` rest param
    let fileName = "RC.apk"; // The default name the browser will use

    res.download(filePath, fileName);
});


module.exports = router;
