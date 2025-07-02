using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class WalletDataContext : SingletonWithGameobject<WalletDataContext>
{
    public WalletData WalletData;

    [HideInInspector]
    public UnityEvent<WalletData> WalletDataChanged;

    public async Task<bool> Initialize()
    {
        return await fetchData();
    }

    public async Task<bool> RefreshData()
    {
        return await fetchData();
    }

    private async Task<bool> fetchData()
    {

        var responce = await APIServices.Instance.GetAsync<Wallet>(APIEndpoints.userWallet, includeAuthorization: true);
        if (responce != null && responce.success)
        {
            WalletData = responce.data;
            WalletDataChanged?.Invoke(WalletData);
            return true;
        }
        else
        {
            Debug.LogError("Unable to Get Wallet Data in Context");
            UnityNativeToastsHelper.ShowShortText("Something went Wrong.");
            return false;
        }
    }



    private void OnDestroy()
    {
        WalletDataChanged.RemoveAllListeners();
    }

}
