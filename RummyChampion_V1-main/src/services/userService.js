const bcrypt = require("bcrypt");
const userRepository = require("../repositories/userRepository");
const { generateOTP } = require("../utils/otpService");
const { sendOTPViaSMS } = require("../utils/otpService");
const jwt = require("../utils/Jwt");
const { ObjectId } = require("mongodb");
const gameRepository = require("../repositories/gameRepository");
const { JWT_SECRET, REFRESH_TOKEN_EXPIRY, ACCESS_TOKEN_EXPIRY } = process.env;

class UserService {
  async signup(mobileNumber, otp, referCode) {
    try {
      // Check if the mobile number is the dummy user
      if (mobileNumber === "1234567890") {
        if (otp === 123456) {
          // Create or fetch the dummy user details
          let dummyUser = await userRepository.findOneByMobileNumber(
            mobileNumber
          );

          if (!dummyUser) {
            const username = "DUMMY_USER_1234";

            const userData = {
              mobileNumber: mobileNumber,
              username: username,
              firsttime: false, // Mark dummy user as not a first-time user
            };

            dummyUser = await userRepository.createUser(userData);
          }

          // Generate JWT tokens
          const jwtData = {
            userId: dummyUser._id,
            mobileNumber: dummyUser.mobileNumber,
            username: dummyUser.username,
          };

          const accessToken = await jwt.generateToken(
            jwtData,
            JWT_SECRET,
            ACCESS_TOKEN_EXPIRY
          );
          const refreshToken = await jwt.generateToken(
            jwtData,
            JWT_SECRET,
            REFRESH_TOKEN_EXPIRY
          );

          return {
            success: true,
            data: {
              userId: jwtData.userId,
              mobileNumber: jwtData.mobileNumber,
              username: jwtData.username,
              accessToken: accessToken,
              refreshToken: refreshToken,
            },
          };
        } else {
          return { success: false, message: "Invalid OTP for dummy user." };
        }
      }

      // Existing logic for other users
      const existingUser = await userRepository.findOneByMobileNumber(
        mobileNumber
      );

      if (existingUser) {
        // User exists, verify OTP
        const find_otp = await userRepository.findOtp(mobileNumber);
        if (!find_otp || find_otp.otp !== otp) {
          return { success: false, message: "Invalid OTP. Please try again." };
        }

        // Valid OTP, proceed with login logic
        const jwtData = {
          userId: existingUser._id,
          mobileNumber: existingUser.mobileNumber,
          username: existingUser.username,
        };

        // Generate access token
        const accessToken = await jwt.generateToken(
          jwtData,
          JWT_SECRET,
          ACCESS_TOKEN_EXPIRY
        );
        const refreshToken = await jwt.generateToken(
          jwtData,
          JWT_SECRET,
          REFRESH_TOKEN_EXPIRY
        );

        return {
          success: true,
          data: {
            userId: jwtData.userId,
            mobileNumber: jwtData.mobileNumber,
            username: jwtData.username,
            accessToken: accessToken,
            refreshToken: refreshToken,
          },
        };
      }

      // Proceed with signup logic since user does not exist
      const username = "LUDO_GAME" + mobileNumber.slice(-4);

      // Prepare user data for registration
      const userData = {
        mobileNumber: mobileNumber,
        username: username,
        firsttime: true, // Assuming this is a new user
      };

      const newUser = await userRepository.createUser(userData);
      const jwtData = {
        userId: newUser._id,
        mobileNumber: newUser.mobileNumber,
        username: newUser.username,
      };

      // Generate access token
      const accessToken = await jwt.generateToken(
        jwtData,
        JWT_SECRET,
        ACCESS_TOKEN_EXPIRY
      );
      const refreshToken = await jwt.generateToken(
        jwtData,
        JWT_SECRET,
        REFRESH_TOKEN_EXPIRY
      );
      const find_otp = await userRepository.findOtp(mobileNumber);
      if (find_otp == null) {
        return {
          success: false,
          message: "Please Send Otp First",
          data: [],
        };
      } else if (find_otp.otp != otp) {
        console.log(" in otp throw");
        return {
          success: false,
          message: "Please provide valid Otp!!",
          data: [],
        };
      }
      return {
        success: true,
        message: "Signup successful Pls Send OTP",
        data: {
          userId: newUser._id,
          mobileNumber: newUser.mobileNumber,
          username: newUser.username,
          firsttime: newUser.firsttime,
          accessToken: accessToken,
          refreshToken: refreshToken,
        },
      };
    } catch (error) {
      console.error("Error in signup service:", error);

      if (error.code === 11000 && error.keyPattern && error.keyValue) {
        // Handle duplicate key error (mobileNumber) specifically
        return {
          success: false,
          message:
            "Mobile number already exists. Please try a different number.",
        };
      }

      // General error handling for other issues
      return {
        success: false,
        message: "Failed to register user. Please try again later.",
      };
    }
  }

  async sendOTP(mobileNumber) {
    try {
      if (mobileNumber === "1234567890") {
        // For the dummy user, save a static OTP
        const staticOtp = "123456";

        // Save the static OTP in the database
        await userRepository.updateOtp(mobileNumber, staticOtp);

        // Log the static OTP (for debugging/testing purposes only)
        console.log(
          `Static OTP for dummy user (${mobileNumber}): ${staticOtp}`
        );

        // return { mobileNumber, otp: staticOtp }; // Return static OTP
      } else {
        // Generate OTP
        const generatedOtp = generateOTP(); // Generate random 6-digit OTP

        // Update OTP in database
        await userRepository.updateOtp(mobileNumber, generatedOtp);

        // Send OTP via mobile marketing (replace with your OTP sending mechanism)
        await sendOTPViaSMS("verification", generatedOtp, mobileNumber);
      }
    } catch (error) {
      console.error("Error sending OTP:", error);
      throw new Error("Failed to send OTP");
    }
  }

  async getProfile(userId) {
    try {
      const profile = await userRepository.findById(userId);
      if (!profile) {
        return { success: false, message: "User doesn't exist!", data: [] };
      }

      // Additional logic for activity calculation
      const activity = {
        played_contest: 0, // Implement calculation logic
        match_played: 0, // Implement calculation logic
        total_series: 0,
      };

      profile.activity = activity;
      return profile;
    } catch (error) {
      return { success: false, message: error.message, data: [] };
    }
  }

  async updateProfile(userId, userData) {
    try {
      const existingUser = await userRepository.findById(userId);
      if (!existingUser) {
        return { success: false, message: "User not found!", data: [] };
      }

      const { username } = userData;
      const isUsernameTaken = await userRepository.findByUsername(username);
      if (isUsernameTaken && isUsernameTaken._id.toString() !== userId) {
        return {
          success: false,
          message: "Username is already in use. Please choose a different one.",
          data: [],
        };
      }

      await userRepository.updateProfile(userId, userData);
      return { success: true, message: "Profile updated.", data: userData };
    } catch (error) {
      return { success: false, message: error.message, data: [] };
    }
  }

  async verify(mobileNumber) {
    try {
      const user_details = await userRepository.findOneByMobileNumber(
        mobileNumber
      );

      return user_details;
    } catch (error) {
      return { success: false, message: error.message, data: [] };
    }
  }

  async getUserProfile(userId) {
    const user = await userRepository.findById(userId);

    if (!user) {
      throw { statusCode: 404, message: "User not found" };
    }

    // const userIdObj = mongoose.Types.ObjectId(userId);

    const games = await gameRepository.getUserGames(userId);
    // console.log(games, ">>>>>>");
    const totalGamesPlayed = games.filter((game) =>
      game.players.some((player) => player.equals(new ObjectId(userId)))
    ).length;
    // console.log(totalGamesPlayed, "::::::");
    const gamesWon = games.filter((game) =>
      game.winner.some((player) => player.equals(new ObjectId(userId)))
    ).length;
    const winRate =
      totalGamesPlayed === 0 ? 0 : (gamesWon / totalGamesPlayed) * 100;
    const totalEarnings = games
      .filter(
        (game) =>
          game.tableId &&
          game.winner.some((player) => player.equals(new ObjectId(userId)))
      )
      .reduce((sum, game) => sum + (game.tableId.wonCoin || 0), 0);
    const player2Wins = games
      .filter(
        (game) =>
          game.tableId &&
          game.tableId.gameType === "2 Player" &&
          game.tableId.game === "Ludo" &&
          game.gameWonDate
      )
      .map((game) => ({
        gameWonDate: game.gameWonDate,
        bet: game.tableId.bet,
        wonCoin: game.tableId.wonCoin,
      }));

    // Filter and map for 3 Player wins
    const player4Wins = games
      .filter(
        (game) =>
          game.tableId &&
          game.tableId.gameType === "4 Player" &&
          game.tableId.game === "Ludo" &&
          game.gameWonDate
      )
      .map((game) => ({
        gameWonDate: game.gameWonDate,
        bet: game.tableId.bet,
        wonCoin: game.tableId.wonCoin,
      }));
    // Filter and map for 3 Player wins
    const player2Rummy = games
      .filter(
        (game) =>
          game.tableId &&
          game.tableId.gameType === "2 Player" &&
          game.tableId.game === "Rummy" &&
          game.gameWonDate
      )
      .map((game) => ({
        gameWonDate: game.gameWonDate,
        bet: game.tableId.bet,
        wonCoin: game.tableId.wonCoin,
      }));
    const player6Rummy = games
      .filter(
        (game) =>
          game.tableId &&
          game.tableId.gameType === "6 Player" &&
          game.tableId.game === "Rummy" &&
          game.gameWonDate
      )
      .map((game) => ({
        gameWonDate: game.gameWonDate,
        bet: game.tableId.bet,
        wonCoin: game.tableId.wonCoin,
      }));

    return {
      user,
      totalEarnings,
      totalGamesPlayed,
      gamesWon,
      winRate,
      player2Wins,
      player4Wins,
      player2Rummy,
      player6Rummy,
    };
  }
}

module.exports = new UserService();
