using System;
using System.Collections.Generic;

[Serializable]
public class TransactionData
{
    public string _id;
    public string userId;
    public string merchantTransactionId { get; set; }
    public double amount;
    public string status;
    public string title;
    public string description;
    public string transactionType;
    public string transactionInto;
    public DateTime createdAt;

}


[System.Serializable]
public class Transactions : BaseModel<List<TransactionData>>
{

}
[System.Serializable]
public class Transaction : BaseModel<TransactionData>
{

}