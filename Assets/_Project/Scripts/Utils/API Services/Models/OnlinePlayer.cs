using System.Collections.Generic;

[System.Serializable]
public class OnlinePlayerData
{
    public string _id;
    public string playerId;
    public string contestId;
}

[System.Serializable]
public class OnlinePlayers : BaseModel<List<OnlinePlayerData>>
{

}