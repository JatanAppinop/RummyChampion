[System.Serializable]
public class UserVerificationData
{
    public string username;
    public string mobileNumber;
    public string userId;
    public string accessToken;
}

[System.Serializable]
public class UserVerification : BaseModel<UserVerificationData>
{

}