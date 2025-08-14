namespace Appinop
{
    public class Constants
    {
        public static readonly string GameName = "Rummy Champions";
        public static readonly string CompanyName = "VIJAYA DHANUSH TECHNOLOGIES PVT LTD";
        public static readonly string SupportNumber = "https://wa.me/919001822280";
        public static readonly string contactWA = "https://wa.me/919001822280";
        public static readonly string ContactEmail = "help@rummychampions.co.in";
        public static readonly string AboutUrl = "https://rummychampions.co.in/";
        public static readonly string HelpURL = "mailto:help@rummychampions.co.in";
        //public static readonly string ServerURL = "https://4268-2401-4900-889b-b11-e55e-f3cb-e105-9263.ngrok-free.app";
        //public static readonly string SocketURL = "https://4268-2401-4900-889b-b11-e55e-f3cb-e105-9263.ngrok-free.app";
        public static readonly string ServerURL = "http://192.168.29.86:9091/";
        //public static readonly string SocketURL = "http://103.175.163.162:9091"; old url 
        public static readonly string SocketURL = "http://192.168.29.86:9091/";  // check new url    
        public static readonly string PrivacyPolicyURL = "https://rummychampions.co.in/privacy-policy.php";
        public static readonly string TermsConditionURL = "https://rummychampions.co.in/terms-and-condition.php";
        public static readonly string SiteURL = "https://rummychampions.co.in/";
        public static readonly string KUserToken = "KUserToken";
        public static readonly string KRefreshToken = "KRefreshToken";
        public static readonly string KUpdatePath = "KUpdatePath";
        public static readonly string KPlayerColorSelect = "KPlayerColorSelect";
        public static readonly string KModeSelect = "KModeSelect";
        public static readonly string KDepositAmount = "KDepositAmount";
        public static readonly string KWithdrawAmount = "KWithdrawAmount";
        public static readonly string KPaymentAccount = "KPaymentAccount";
        public static readonly string KSoundtoggle = "KSoundtoggle";
        public static readonly string KMusictoggle = "KMusictoggle";
        public static readonly string KMatchId = "KMatchId";
        public static readonly string KContestId = "KMatchId";
        public static readonly string KGameMode = "KGameMode";
        public static readonly string KSelectedGame = "KSelectedGame";
        public static readonly string KLastTID = "KLastTID";
        public static readonly float TurboModeTime = 5 * 60f;
        public static readonly float FourPTurboModeTime = 7 * 60f;
        public static readonly float TSectionColorChange = 0.5f;
        public static readonly float TTurnTimeout = 15f;
        public static readonly float TurnTimeout = 10f;
        public static readonly float TTokenMoveDuration = 0.15f;
        public static readonly float TTokenMoveOffset = 0.2f;
        public static readonly float TTokenJumpPower = 40f;
        public static readonly float TTokenRemoveOffset = 0.05f;
        public static readonly float TTokenEffectTimeout = 1.2f;
        public static readonly float TTokenRemoveEffectTimeout = 0.1f;
        public static readonly float THandMovement = 0.4f;
        public static readonly int TotalNumberMoved = 30;
        public static readonly string KTwoPlayer = "2 Player";
        public static readonly string KFourPlayer = "4 Player";

        public enum GameMode
        {
            Classic,
            Turbo,
            Timer

        }

        public enum PlayerCounts
        {
            TwoPlayer = 2,
            FourPlayer = 4
        }

    }
}