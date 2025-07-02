using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class OnlinePlayersContext : SingletonWithGameobject<OnlinePlayersContext>
{
    [field: SerializeField] public List<OnlinePlayerData> onlinePlayersData { get; private set; } = new List<OnlinePlayerData>();

    [HideInInspector]
    public UnityEvent OnDataUpdated;

    public void Initialize()
    {
        StartCoroutine(startAutoUpdate());
    }
    public async Task<bool> FetchData()
    {
        var response = await APIServices.Instance.GetAsync<OnlinePlayers>(APIEndpoints.getOnlinePlayers);
        if (response != null && response.success)
        {
            onlinePlayersData = response.data;
            OnDataUpdated?.Invoke();
        }
        else
        {
            Debug.LogError("Error Fetching Online Players Data");
            return false;
        }
        return true;
    }

    private IEnumerator startAutoUpdate()
    {
        while (true)
        {
            yield return FetchData();
            yield return new WaitForSeconds(2f);
        }
    }

    public int GetOnlinePlayersCountForMode(string mode)
    {
        var ludoTableIds = new HashSet<string>(DataContext.Instance.contestsData
        .Where(c => c.gameMode == mode)
        .Select(c => c._id));

        return onlinePlayersData.Count(u => ludoTableIds.Contains(u.contestId));

    }
    public int GetOnlinePlayersCountForContest(string contestId)
    {
        return onlinePlayersData.Count(u => u.contestId == contestId);
    }
    public int GetOnlinePlayersCountForLudo()
    {

        var ludoTableIds = new HashSet<string>(DataContext.Instance.contestsData
        .Where(c => c.game == "Ludo")
        .Select(c => c._id));

        return onlinePlayersData.Count(u => ludoTableIds.Contains(u.contestId));
    }
    public int GetOnlinePlayersCountForRummy()
    {
        var rummyTableIds = new HashSet<string>(DataContext.Instance.contestsData
        .Where(c => c.game == "Rummy")
        .Select(c => c._id));

        return onlinePlayersData.Count(u => rummyTableIds.Contains(u.contestId));
    }

    private void OnDestroy()
    {
        OnDataUpdated.RemoveAllListeners();
    }


}