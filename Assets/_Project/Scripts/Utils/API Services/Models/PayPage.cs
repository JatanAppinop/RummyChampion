using System;
using System.Collections.Generic;
[System.Serializable]

public class InstrumentResponse
{
    public string type { get; set; }
    public string intentUrl { get; set; }
    public RedirectInfo redirectInfo { get; set; }
}
[System.Serializable]

public class RedirectInfo
{
    public string url { get; set; }
    public string method { get; set; }
}
[System.Serializable]
public class PayPageData
{
    public string merchantId { get; set; }
    public string merchantTransactionId { get; set; }
    public InstrumentResponse instrumentResponse { get; set; }

}
[System.Serializable]
public class PayPageData2
{
    public string base64Payload { get; set; }
    public string checksum { get; set; }
    public string transactionId { get; set; }

}


[System.Serializable]
public class PayPage : BaseModel<PayPageData>
{

}
[System.Serializable]
public class PayPage2 : BaseModel<PayPageData2>
{

}