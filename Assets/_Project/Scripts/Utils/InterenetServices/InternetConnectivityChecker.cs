using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;

public static class InternetConnectivityChecker
{
    public static async Task<bool> CheckInternetAsync()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://www.google.com"))
        {
            // webRequest.timeout = 2;
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Delay(10);
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
