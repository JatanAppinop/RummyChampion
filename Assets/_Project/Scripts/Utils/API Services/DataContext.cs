using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class DataContext : SingletonWithGameobject<DataContext>
{
    public List<TableData> contestsData;


    [HideInInspector]
    public UnityEvent<List<TableData>> OnContestDataUpdated;


    public async Task<bool> FetchData()
    {


        var response = await APIServices.Instance.GetAsync<ContestTable>(APIEndpoints.getContestsTable);
        if (response != null && response.success)
        {
            contestsData = response.data;
        }
        else
        {
            Debug.LogError("Error Fetching Contests Data");
            return false;
        }


        return true;
    }

    public async Task<bool> RefreshData()
    {
        var response = await FetchData();
        OnContestDataUpdated?.Invoke(contestsData);

        return response;
    }
    private void OnDestroy()
    {
        OnContestDataUpdated.RemoveAllListeners();
    }
}