// controllers/playerController.js
const OnlinePlayer = require("../models/OnlinePlayers");

// Get all online players
async function getAllOnlinePlayers(req, res) {
  try {
    const players = await OnlinePlayer.find({});
    return res.status(200).json({
      success: true,
      message: "Get Online Player Successful",
      data: players,
    });
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
};

// Get players by contestId
async function getOnlinePlayersByContestId(req, res) {
  const { contestId } = req.query;
  console.log("🚀 ~ getOnlinePlayersByContestId ~ contestId:", contestId)
  try {
    const players = await OnlinePlayer.find({ contestId });
    return res.status(200).json({
      success: true,
      message: "Get Online Player Successful",
      data: players,
    });
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
};

module.exports = {
  getAllOnlinePlayers,
  getOnlinePlayersByContestId,
};
