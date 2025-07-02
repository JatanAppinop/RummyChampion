[System.Serializable]
class PaymentSettings : BaseModel<PaymentSettingsData>
{

}
[System.Serializable]
public class PaymentSettingsData
{
    public double sgst = 18.5;
    public double cgst = 18.5;
    public double tds = 10;
    public double withdrawCharge = 10;
    public double bonus = 27;
}
