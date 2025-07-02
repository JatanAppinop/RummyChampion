using System;
using Newtonsoft.Json;

public class CashfreePaymentResponse
{
    public bool success { get; set; }
    public string message { get; set; }
    public PaymentData data;
}

public class PaymentData
{
    public long cf_order_id { get; set; }
    public DateTime created_at { get; set; }
    public CustomerDetails customer_details { get; set; }
    public string entity { get; set; }
    public decimal order_amount { get; set; }
    public string order_currency { get; set; }
    public DateTime order_expiry_time { get; set; }
    public string order_id { get; set; }
    public OrderMeta order_meta { get; set; }
    public string order_note { get; set; }
    public string order_status { get; set; }
    public string order_token { get; set; }
    public string payment_link { get; set; }
    public PaymentUrl payments { get; set; }
    public PaymentUrl refunds { get; set; }
    public PaymentUrl settlements { get; set; }
}

public class CustomerDetails
{
    public string customer_id { get; set; }
    public string customer_name { get; set; }
    public string customer_email { get; set; }
    public string customer_phone { get; set; }
    public string customer_uid { get; set; }
}

public class OrderMeta
{
    public string return_url { get; set; }
    public string notify_url { get; set; }
    public string payment_methods { get; set; }
}

public class PaymentUrl
{
    public string url { get; set; }
}