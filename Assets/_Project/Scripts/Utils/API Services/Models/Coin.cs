using System.Collections.Generic;

[System.Serializable]
public class CoinItem
{
    public string planName { get; set; }
    public int price { get; set; }
    public int coin { get; set; }
    public bool isActive { get; set; }
}


[System.Serializable]
public class CoinList : BaseModel<List<CoinItem>>
{

}