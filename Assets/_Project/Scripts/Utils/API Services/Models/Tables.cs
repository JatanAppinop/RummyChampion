using System;
using System.Collections.Generic;

[Serializable]
public class TableData
{
    public string _id;
    public double bet;
    public double totalBet;
    public double rake;
    public double rakePercentage;
    public double wonCoin;
    public bool isActive;
    public string gameMode; //classic
    public string gameType; //2 Player / 4 Player
    public string game; //Ludo Rummy
    public double pointValue = 0; //Ludo Rummy


}

[Serializable]
class ContestTable : BaseModel<List<TableData>>
{

}
