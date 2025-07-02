using System.Collections.Generic;

[System.Serializable]
public class BannerItem
{
    public string _id { get; set; }
    public string url { get; set; }
    public string image { get; set; }
    public bool isActive { get; set; }
    public string bannerType { get; set; }
    public int? bannerSequence { get; set; }

}


[System.Serializable]
public class Banners : BaseModel<List<BannerItem>>
{

}