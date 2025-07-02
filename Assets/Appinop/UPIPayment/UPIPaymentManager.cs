using System;
using UnityEngine;

namespace Appinop.UPIPayment
{


    public class UPIPaymentManager : MonoBehaviour
    {
        private static UPIPaymentManager instance;

        public static UPIPaymentManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UPIPaymentManager>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<UPIPaymentManager>();
                        singletonObject.name = "UPIPaymentManager";
                    }
                }
                return instance;
            }
        }




        private event Action<string> TransactionFinishedCallback;
        public UPIPaymentManager StartUPITransaction(string upiId, string name, string amount, string description, string transactionRefId, string transactionId, string merchantCode, string sign)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", currentActivity, new AndroidJavaClass("com.appinop.upipayment.UPIPaymentActivity")))
        {
            intent.Call<AndroidJavaObject>("putExtra", "upiId", upiId);
            intent.Call<AndroidJavaObject>("putExtra", "name", name);
            intent.Call<AndroidJavaObject>("putExtra", "amount", amount);
            intent.Call<AndroidJavaObject>("putExtra", "description", description);
            intent.Call<AndroidJavaObject>("putExtra", "transactionRefId", transactionRefId);
            intent.Call<AndroidJavaObject>("putExtra", "transactionId", transactionId);
            intent.Call<AndroidJavaObject>("putExtra", "mc", merchantCode);
            intent.Call<AndroidJavaObject>("putExtra", "sign", sign);

            currentActivity.Call("startActivity", intent);
        }
#endif

            return this;
        }

        public UPIPaymentManager OnSuccess(Action<string> onSuccessCallback)
        {
            TransactionFinishedCallback = onSuccessCallback;
            return this;
        }

        private void OnTransactionComplete(string data)
        {
            Debug.Log("Transaction Responce : " + data);
            TransactionFinishedCallback?.Invoke(data);
            DestroySelf();
        }

        private void DestroySelf()
        {
            //Destroy(gameObject);
            //instance = null;
        }
    }
}
