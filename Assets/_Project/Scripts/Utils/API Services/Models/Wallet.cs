using System;
using System.Collections.Generic;

[System.Serializable]
public class WalletData
{
    public double totalBalance { get; set; }
    public double winningBalance { get; set; }
    public double depositBalance { get; set; }
    public double cashBonus { get; set; }
    public double bonus { get; set; }
    public double minimumWithdraw { get; set; }
    public double maximumWithdraw { get; set; }
}

[System.Serializable]
public class Wallet : BaseModel<WalletData>
{

}