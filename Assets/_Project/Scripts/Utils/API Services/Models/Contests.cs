using System;
using System.Collections.Generic;

[Serializable]
public class ContestDetail
{
    public int winningAmount { get; set; }
    public int entryFee { get; set; }
    public int online { get; set; }
    public string _id { get; set; }
}

[Serializable]
public class ContestData
{
    public string _id { get; set; }
    public string gameMode { get; set; }
    public string playerCount { get; set; }
    public List<ContestDetail> contestDetails { get; set; }

}

[Serializable]
class Contests : BaseModel<List<ContestData>>
{

}