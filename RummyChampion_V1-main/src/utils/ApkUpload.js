const multer = require("multer");
const mime = require("mime");
const path = require('path');
const fs = require('fs');

const filePath = 'public/uploads/';
const storage = multer.diskStorage({
    destination: function (req, file, cb) {
        if (!fs.existsSync(filePath)) {
            fs.mkdirSync(filePath)
        }
        cb(null, filePath)
    },

    filename: function (req, file, cb) {
        cb(
            null,
            file.fieldname +
            "." +
            mime.getExtension(file.mimetype)
        );
    },
});

module.exports = {
    uploadapk: multer({
        storage: storage,
        limits: {
            fieldSize: 200 * 1024 * 1024,
        },
        fileFilter(req, file, cb) {
            if (!file.originalname.match(/\.(apk)$/)) {
                return cb(new Error('Allowed only apk file'))
            }
            cb(undefined, true)
        }
    }),

}