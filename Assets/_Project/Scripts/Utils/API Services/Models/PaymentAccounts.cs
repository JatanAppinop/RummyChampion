using System.Collections.Generic;

[System.Serializable]
public class Vpa
{
    public string address { get; set; }
    public string username { get; set; }
}
[System.Serializable]
public class BankAccount
{
    public string ifsc { get; set; }
    public string name { get; set; }
    public List<object> notes { get; set; }
    public string account_number { get; set; }
}
[System.Serializable]
public class PaymentAccount
{
    public BankAccount bankAccount { get; set; }
    public Vpa vpa { get; set; }
    public string _id { get; set; }
    public string userId { get; set; }
    public string fundAccountId { get; set; }
    public string entity { get; set; }
    public string contactId { get; set; }
    public string accountType { get; set; }
    public bool active { get; set; }
    public int __v { get; set; }
}

[System.Serializable]
public class PaymentAccounts : BaseModel<List<PaymentAccount>>
{

}