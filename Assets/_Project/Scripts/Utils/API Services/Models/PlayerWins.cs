using System;
using System.Collections.Generic;

[Serializable]
class PlayerWinsData
{
    public string _id { get; set; }
    public string winRank { get; set; }
    public int winAmount { get; set; }
    public string gameBeat { get; set; }
    public DateTime date { get; set; }

}

[Serializable]
class PlayerWins : BaseModel<List<PlayerWinsData>>
{

}