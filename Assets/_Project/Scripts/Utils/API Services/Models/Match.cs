using System;
using System.Collections.Generic;

[Serializable]
public class MatchData
{
    public string _id;
    public string tableId;
    public List<string> players;
    public DateTime gameStartedDate;
    public List<object> winner;
    public int __v;

}

[Serializable]
class Match : BaseModel<MatchData>
{

}
