const mongoose = require('mongoose');
const { ObjectId } = mongoose.Schema.Types;

const gameSchema = new mongoose.Schema({
    players: [{
        type: ObjectId,
        ref: 'User',
        required: true
    }],
    tableId: {
        type: ObjectId,
        ref: 'Table',
        required: true
    },
    winner: [{
        type: ObjectId,
        ref: 'User'
    }],
    gameStartedDate: {
        type: Date,
        required: true,
        default: Date.now
    },
    gameWonDate: {
        type: Date
    }
}, {
    timestamps: true,
    versionKey: false
});

const Game = mongoose.model('Game', gameSchema);

module.exports = Game;
