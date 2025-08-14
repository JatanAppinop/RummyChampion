module.exports = {
    "databases": {
        "development": process.env.DEV_DATABASE_URI,
        "production": process.env.PROD_DATABASE_URI,
    }
}