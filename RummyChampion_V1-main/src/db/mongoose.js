const mongoose = require("mongoose");
mongoose.set("strictQuery", true);
mongoose.set("debug", false);

let connectAttempts = 0;
const maxConnectAttempts = 5; // Maximum number of connection attempts
const connectRetryInterval = 5000; // Retry connection after 5 seconds

const dbconnect = (uri) => {
  console.log(uri, ": this is the database URL");

  const connectWithRetry = () => {
    console.log("Attempting MongoDB connection (attempt ", connectAttempts + 1, ")");

    mongoose.connect(uri)
      .then(() => {
        console.log("Connected to MongoDB");
        connectAttempts = 0; // Reset connection attempts counter
      })
      .catch((err) => {
        console.error("MongoDB connection error:", err.message);
        if (connectAttempts < maxConnectAttempts) {
          console.log(`Retrying connection in ${connectRetryInterval / 1000} seconds...`);
          connectAttempts++;
          setTimeout(connectWithRetry, connectRetryInterval);
        } else {
          console.error(`Maximum connection attempts (${maxConnectAttempts}) reached. Exiting...`);
          process.exit(1); // Exit the application after too many failed attempts
        }
      });
  };

  connectWithRetry();

  mongoose.connection.on("error", (err) => {
    console.error("MongoDB connection error:", err.message);
  });
};

module.exports = {
  dbconnect
};
