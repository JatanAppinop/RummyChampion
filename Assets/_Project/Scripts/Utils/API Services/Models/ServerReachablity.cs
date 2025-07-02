using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ServerReachablityData
{
    public bool maintenance;
    public string appVersion = Application.version;

}

[System.Serializable]
public class ServerReachablity : BaseModel<ServerReachablityData>
{

}