using System.Collections.Generic;

[System.Serializable]
public class UserData
{
    public string _id;
    public string mobileNumber;
    public string username;
    public string status;
    public string type;
    public int verified;
    public int emailVerified;
    public string kycVerified;
    public int mobileVerified;
    public object refercode;
    public double cashBonus;
    public double totalBalance;
    public double winningBalance;
    public double depositBalance;
    public object fcmToken;
    public object fcmDevice;
    public bool firsttime;

    public double totalCoins;
    public double walletCoins;


    public int profilePhotoIndex;
    public int backdropIndex;

    public int TwoPlayer_Win;
    public int FourPlayer_Win;

}

[System.Serializable]
public class User : BaseModel<UserData>
{

}