const mongoose = require("mongoose");
const { ObjectId } = require("bson");

const transactionSchema = new mongoose.Schema(
  {
    userId: {
      type: ObjectId,
      ref: "User",
      // required: true,
      index: true,
    },
    adminId: {
      type: ObjectId,
      ref: "Admins",
    },
    merchantTransactionId: { type: String },
    transactionId: { type: String },
    paymentInstrument: { type: Object },
    orderId: { type: String },
    amount: { type: Number },
    previousWinningBalance: {
      type: Number,
    },
    previousTotalBalance: {
      type: Number,
    },
    previousCashBonus: {
      type: Number,
      // required: true
    },
    currentWinningBalance: {
      type: Number,
      // required: true
    },
    currentTotalBalance: {
      type: Number,
      // required: true
    },
    currentCashBonus: {
      type: Number,
      // required: true
    },
    status: {
      type: String,
      required: true,
    },
    /**
     * Pending
     * Rejected
     * Approved
     */
    title: {
      type: String,
      required: true,
    },
    description: {
      type: String,
      required: true,
    },
    gameId: {
      type: ObjectId,
      ref: "Game",
    },
    gameName: { type: String },
    eventType: {
      type: String,
      enum: ["Type1", "Type2", "Type3"],
    },
    tableId: { type: String },
    previousCoins: { type: Number },
    currentCoins: { type: Number },
    GSTAmount: { type: Number },
    transactionType: { type: String },
    transactionInto: { type: String },
    isDeductTds: { type: Boolean },
    gameStatus: { type: String },
    playedGameId: {
      type: ObjectId,
    },
    refundTransactionId: {
      type: ObjectId,
    },
    tdsId: {
      type: ObjectId,
    },
    lossAmount: { type: Number },
    refundAmount: { type: Number },
    numericId: {
      type: Number,
      default: 0,
      index: true,
    },
    gstDetails: {
      igstPercentage: { type: Number, default: 0 },
      sgstPercentage: { type: Number, default: 0 },
      totalGstPercentage: { type: Number, default: 0 },
      igstAmount: { type: Number, default: 0 },
      cgstAmount: { type: Number, default: 0 },
      totalGstAmount: { type: Number, default: 0 },
    },
    tdsDetails: {
      tdsPercentage: { type: Number },
      tdsAmount: { type: Number },
    },
    netAmount: { type: Number },
  },
  {
    timestamps: true,
    versionKey: false,
    toJSON: {
      virtuals: true,
      getters: true,
    },
  }
);

transactionSchema.pre("save", async function (next) {
  const maxNumber = await Transaction.findOne().sort({ numericId: -1 });
  let maxNumberId = 1;

  if (maxNumber && maxNumber.numericId) {
    maxNumberId = maxNumber.numericId + 1;
  }

  this.numericId = maxNumberId;
  next();
});

const Transaction = mongoose.model("Transaction", transactionSchema);

module.exports = Transaction;
module.exports = mongoose.model("Transaction", transactionSchema);
