using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class SettingsDataContext : SingletonWithGameobject<SettingsDataContext>
{
    public PaymentSettingsData PaymentSettings;

    [HideInInspector]
    public UnityEvent<PaymentSettingsData> PaymentSettingsDataChanged;

    public async void Initialize()
    {
        await fetchData();
    }

    public async Task<bool> RefreshData()
    {
        return await fetchData();
    }

    private async Task<bool> fetchData()
    {
        var responce = await APIServices.Instance.GetAsync<PaymentSettings>(APIEndpoints.paymentSettings, includeAuthorization: true);
        if (responce != null && responce.success)
        {
            PaymentSettings = responce.data;
            PaymentSettingsDataChanged?.Invoke(PaymentSettings);
            return true;
        }
        else
        {
            Debug.LogError("Unable to Payment Data in Context");
            UnityNativeToastsHelper.ShowShortText("Something went Wrong.");
            return false;
        }
    }

    private void OnDestroy()
    {
        PaymentSettingsDataChanged.RemoveAllListeners();
    }

}

