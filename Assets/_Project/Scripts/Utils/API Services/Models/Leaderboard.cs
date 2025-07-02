using System.Collections.Generic;

[System.Serializable]
public class LeaderboardItem
{
    public int wins { get; set; }
    public int rank { get; set; }
    public string userId { get; set; }
    public string username { get; set; }
    public int profilePhotoIndex;
    public int backdropIndex;

}


[System.Serializable]
public class LeaderboardData : BaseModel<List<LeaderboardItem>>
{

}