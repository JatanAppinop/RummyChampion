const mongoose = require("mongoose");

const OnlinePlayersSchema = new mongoose.Schema({
  playerId: { type: String, required: true, unique: true },
  contestId: { type: String, required: true },
});

const OnlinePlayers = mongoose.model("OnlinePlayers", OnlinePlayersSchema);

module.exports = OnlinePlayers