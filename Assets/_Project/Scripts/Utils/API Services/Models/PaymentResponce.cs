// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class NotificationResponse
{
    public string code { get; set; }
    public string merchantId { get; set; }
    public string transactionId { get; set; }
    public string amount { get; set; }
    public string providerReferenceId { get; set; }
    public string checksum { get; set; }
}

public class PaymentResponce
{
    public string url { get; set; }
    public string callMode { get; set; }
    public NotificationResponse notificationResponse { get; set; }
}

