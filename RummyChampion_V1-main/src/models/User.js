const { required } = require("joi");
const mongoose = require("mongoose");

const userSchema = new mongoose.Schema({
    mobileNumber: {
        type: String,
        required: true,
        unique: true, // Ensures unique mobile numbers
    },
    name: {
        type: String,
        required: false
    },
    email: {
        type: String,
        required: false
    },
    username: {
        type: String,
        required: true
    },
    password: {
        type: String,
        required: false
    }, // Add password field if needed
    status: {
        type: String,
        default: "Active",
        required: true
    },
    type: {
        type: String,
        default: "customer"
    },
    verified: {
        type: Number,
        default: 0,
        enum: [0, 1, 2]
    }, // 2 for pending
    emailVerified: {
        type: Number,
        default: 0,
        enum: [0, 1, 2]
    }, // 2 for pending
    kycSubmitted: { type: Boolean },
    kycVerified: {
        type: "String",
        default: "Not Applied",
        required: true
    },
    mobileVerified: {
        type: Number,
        default: 1,
        enum: [0, 1, 2]
    }, // 2 for pending
    refercode: {
        type: String,
        default: null
    },
    cashBonus: {
        type: Number,
        default: 0
    },
    totalBalance: {
        type: Number,
        default: 0
    },
    winningBalance: {
        type: Number,
        default: 0
    },
    depositBalance: {
        type: Number,
        default: 0
    },
    fcmToken: {
        type: String,
        default: null
    },
    fcmDevice: {
        type: String,
        default: null
    },
    firsttime: {
        type: Boolean,
        default: true
    }, // Track if it's the first time user
    profilePhotoIndex: {
        type: Number,
    },
    backdropIndex: {
        type: Number,
    }
},
    /**
     * status for verification 
     * Pending
     * Rejected
     * Approved
     * NotApplied
     */
    {
        timestamps: true,
        toJSON: {
            virtuals: true,
        },
        toObject: {
            virtuals: true,
        },
    });

userSchema.pre('save', function (next) {
    this.totalBalance = this.cashBonus + this.winningBalance + this.depositBalance;
    next();
});

userSchema.virtual('totalCoins').get(function () {
    return Math.floor(this.totalBalance); // Assuming each coin is equivalent to 1 INR
});


const User = mongoose.model("User", userSchema);

module.exports = User;
