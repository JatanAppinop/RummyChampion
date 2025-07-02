using System;
using System.Collections.Generic;

[System.Serializable]
public class WalletPageData
{
    public string _id { get; set; }
    public string user_id { get; set; }
    public string status { get; set; }
    public string currency { get; set; }
    public string transactionType { get; set; }
    public string short_name { get; set; }
    public string icon_path { get; set; }
    public int TotalBalance { get; set; }
    public int WinningBalance { get; set; }
    public int DepositeBalance { get; set; }
    public int CashBack { get; set; }
    public int Bonus { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public int __v { get; set; }
}

[System.Serializable]
public class WalletPage : BaseModel<List<WalletPageData>>
{

}