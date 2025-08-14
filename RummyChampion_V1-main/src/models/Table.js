const { required } = require("joi");
const mongoose = require("mongoose");

const gameEnum = ["Ludo", "Rummy"];
const gameModeEnum = [
  "Classic",
  "Timer",
  "Turbo",
  "Points",
  "Deals",
  "101 Pool",
  "201 Pool",
];
const gameTypeEnum = ["2 Player", "4 Player", "6 Player"];

const tableSchema = new mongoose.Schema(
  {
    bet: {
      type: Number,
      required: true,
    },
    totalBet: {
      type: Number,
      required: true,
    },
    rake: {
      type: Number,
      required: true,
    },
    rakePercentage: {
      type: Number,
      required: true,
    },
    wonCoin: {
      type: Number,
      required: true,
    },
    createdOn: {
      type: Date,
      default: Date.now,
    },
    isActive: {
      type: Boolean,
      default: true,
    },
    gameMode: {
      // add game mode
      type: String,
      enum: gameModeEnum,
      required: true,
    },
    gameType: {
      // add game type
      type: String,
      enum: gameTypeEnum,
      required: true,
    },
    game: {
      type: String,
      enum: gameEnum,
      required: true,
    },
    pointValue: {
      type: Number,
      required: false,
    },
  },
  {
    timestamps: true,
    versionKey: false,
  }
);

const Table = mongoose.model("Table", tableSchema);

module.exports = Table;
