static class APIEndpoints
{
    //Used API
    public static string verifyToken = "user/verifytoken";
    public static string sentOtp = "user/sendOtp";
    public static string loginAndSignup = "user/signup";
    public static string userProfile = "user/profile";
    public static string editProfile = "user/editProfile";
    public static string playerProfile = "user/playerProfile";
    public static string getTransactions = "user/transactions";



    public static string getContestsTable = "user/allTables";
    public static string getOnlinePlayers = "OnlinePlayers/getAllPlayers";
    public static string leaderboard = "user/leaderboard";
    public static string userWallet = "wallet/userWallet";
    public static string requestDeposit = "user/requestDeposit";
    public static string checkTransaction = "user/checkTransaction/";
    public static string requestDepositTest = "user/requestDepositTest";
    public static string getFunAccounts = "user/getFundAccount";
    public static string addBankAccount = "user/createFunds&Pay";
    public static string requestWithdrawal = "user/requestWithdrawal";
    public static string getMatch = "user/getGame/";
    public static string getMatches = "user/games/";
    public static string getUser = "user/userProfiles/";
    public static string getCoins = "user/shopPlans";
    public static string submitKYC = "user/submitKyc";
    public static string getBanner = "user/banners";
    public static string downloadAPK = "downloads";
    public static string gameSettings = "admin/settings";
    public static string paymentSettings = "transcationSettings";

    //User Automatic kyc
    public static string aadharOtp = "user/aadharOtp";
    public static string verifyAadhar = "user/verifyAadhar";
    public static string submitPan = "user/submitPan";
    public static string requestCashFree = "user/requestCashFree";
    public static string checkTransactionCashFree = "user/cash-free-payment-status";




    //Non Verified
    public static string addBankDetail = "user/user_bank_details";
    public static string GetBankDetail = "user/get_user_bank_details";
    public static string walletPage = "wallet/user-wallet";
    public static string getTwoPlayerWins = "game/getTwoplayerWin";
    public static string getFourPlayerWins = "game/getFourplayerWin";
    public static string getSubmitKYC = "user/getsubmitkyc";

}