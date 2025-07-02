#if UNITY_EDITOR || UNITY_ANDROID
using System;
using UnityEngine;

namespace Appinop.PhonePePlugin
{
    public class AndroidPhonePePlatform : IPhonePePlatform
    {
        private static AndroidJavaClass phonePeHelper;
        private static AndroidJavaObject unityActivity;

        public AndroidPhonePePlatform()
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            phonePeHelper = new AndroidJavaClass("com.appinop.phonepeupi.PhonePeHelper");
        }

        public void InitializePhonePe(string environment, string merchantId, string appSchema)
        {
            if (Enum.TryParse(environment, out PhonePeHelper.ENVIRONMENT env))
            {
                switch (env)
                {
                    case PhonePeHelper.ENVIRONMENT.SANDBOX:
                        environment = "SANDBOX";
                        break;
                    case PhonePeHelper.ENVIRONMENT.PRODUCTION:
                        environment = "RELEASE";
                        break;
                    default:
                        environment = "RELEASE";
                        break;
                }

            }

            phonePeHelper.CallStatic("initializePhonePe", unityActivity, environment, merchantId);
            Debug.Log("PhonePe initialized on Android.");
        }

        public void StartTransaction(string base64Payload, string checksum)
        {
            phonePeHelper.CallStatic("startTransaction", unityActivity, base64Payload, checksum, "/pg/v1/pay");
            Debug.Log("Transaction started on Android.");
        }
    }
}

#endif