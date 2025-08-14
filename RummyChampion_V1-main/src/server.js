const express = require("express");
const app = express();
const helmet = require("helmet");
require("dotenv").config({ path: "./src/config/.env" });
const config = require("./config/config");
const { PORT, ENV } = process.env;
const { dbconnect } = require("./db/mongoose");
dbconnect(config["databases"][ENV]);
const errorLogger = require("./utils/ErrorLogger");
const userRoutes = require("./routes/userRoutes");
const transactionRoutes = require("./routes/transactionRoutes");
const adminRoutes = require("./routes/adminRoutes");
const shopPlanRoutes = require("./routes/shopPlanRoutes");
const tableRoutes = require("./routes/tableRoutes");
const gameRoutes = require("./routes/gameRoutes");
const kycRoutes = require("./routes/kycRoutes");
const gameService = require("./services/gameService");
const gstRoutes = require("./routes/gstRoutes");
const notify = require("./routes/notificationRoutes");
const onlinePlayers = require("./routes/OnlinePlayersRoutes");
const path = require("path");
const fs = require("fs");
const cors = require("cors");

const { connect, inititate } = require("./config/newWS");
const expressWinston = require("express-winston");
const logger = require("./utils/Logger");

const routesToIgnore = ["/OnlinePlayers/getAllPlayers"];

// Middleware for logging all requests

app.use(
  expressWinston.logger({
    winstonInstance: logger,
    meta: true,
    msg: "HTTP {{req.method}} {{req.url}}",
    expressFormat: true,
    colorize: true,
    ignoreRoute: function (req, res) {
      if (routesToIgnore.includes(req.url)) {
        return true;
      }
      return false;
    },
  })
);

// Your routes here

// Middleware for logging errors
app.use(
  expressWinston.errorLogger({
    winstonInstance: logger,
  })
);

app.get("/", async (req, res) => {
  const { maintenance } = await gameService.getSettings();
  return res.json({
    success: true,
    message: "RC Server is online",
    data: { maintenance: maintenance },
  });
});

// Error handling middleware
app.use((err, req, res, next) => {
  logger.error(err.stack);
  res.status(500).send("Something broke!");
});

app.use(express.json({ limit: "25mb" }));
app.use(
  express.urlencoded({ limit: "25mb", extended: true, parameterLimit: 200000 })
);

app.use((req, res, next) => {
  res.header("Access-Control-Allow-Origin", "*");
  res.header(
    "Access-Control-Allow-Methods",
    "GET,HEAD,OPTIONS,POST,PUT,PATCH,DELETE"
  );
  res.header(
    "Access-Control-Allow-Headers",
    "Origin, X-Requested-With, Content-Type, Accept, Authorization"
  );
  next();
});

const uploadDir = path.join(__dirname, "uploads");
if (!fs.existsSync(uploadDir)) {
  fs.mkdirSync(uploadDir, { recursive: true });
}

app.use(cors());
app.use(helmet());
app.set("view engine", "ejs");

app.use(express.static("public"));
app.use([userRoutes], errorLogger);
app.use([transactionRoutes], errorLogger);
app.use([adminRoutes], errorLogger);
app.use([shopPlanRoutes], errorLogger);
app.use([tableRoutes], errorLogger);
app.use([gameRoutes], errorLogger);
app.use([kycRoutes], errorLogger);
app.use([gstRoutes], errorLogger);
app.use([notify], errorLogger);
app.use([onlinePlayers], errorLogger);

app.use("/public", (req, res) => {
  const path = `${process.cwd()}/public${req.url}`;
  if (fs.existsSync(path)) return res.sendFile(path);
  else return res.status(404).send("File doesn't exists!");
});

const { server, io } = inititate(app);

server.listen(PORT, async () => {
  console.log("Server is up on port:", PORT);
  connect(io);
  // process.on("unhandledRejection", (reason, p) => {
  //     // We just caught an unhandled promise rejection, since we already have fallback handler for unhandled errors (see below), let throw and let him handle that
  //     console.log(p);
  //     throw reason;
  // });
  // // const { fork } = require("child_process");

  // // const childProcess = fork("./src/utils/refresh-match.js");
  // // console.log("we are going to initiate the child process.");
  // // const startTime = new Date();
  // // // the first argument to fork() is the name of the js file to be run by the child process
  // // childProcess.send({ data: 1 }); //send method is used to send message to child process through IPC
  // // console.log("we have send the request.");
  // // // childProcess.stdout.pipe(childProcess.stdout);
  // // console.log("stdout: ", childProcess.stdout);
  // // childProcess.on("message", (message) => {
  // //     console.log("we have got the message.");
  // //     //on("message") method is used to listen for messages send by the child process
  // //     const endTime = new Date();
  // //     console.log(
  // //         "Process took " + (endTime.getTime() - startTime.getTime()) + "ms"
  // //     );
  // // });

  // // childProcess.on("close", (code) => {
  // //     console.log(`child process exited with code ${code}`);
  // // });
  // // childProcess.on("exit", function () {
  // //     console.log(`exit child process exited with code`);
  // //     childProcess.stdin.pause();
  // //     childProcess.kill();
  // // });
  // // childProcess.on("error", (err) => {
  // //     console.log(`exit child process exited with code`, err);
  // //     // This will be called with err being an AbortError if the controller aborts
  // // });

  // process.on("uncaughtException", (error) => {
  //     console.log(error);
  // });
});
