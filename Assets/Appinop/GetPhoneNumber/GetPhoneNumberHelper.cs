using System;
using UnityEngine;

namespace Appinop.GetPhoneNumber
{


    public class GetPhoneNumberHelper : MonoBehaviour
    {
        private static GetPhoneNumberHelper instance;

        public static GetPhoneNumberHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GetPhoneNumberHelper>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<GetPhoneNumberHelper>();
                        singletonObject.name = "GetPhoneNumber_Helper";
                    }
                }
                return instance;
            }
        }


        private event Action<string> PhoneNumberReceived;
        private event Action<string> PhoneNumberError;
        public GetPhoneNumberHelper GetNumber()
        {
#if UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            // AndroidJavaClass phoneNumberActivityClass = new AndroidJavaClass("com.appinop.phonenumberselectorhelper.PluginActivity");
            AndroidJavaClass phoneNumberActivityClass = new AndroidJavaClass("com.appinop.autoreadotp.PhoneNumberActivity");

            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", currentActivity, phoneNumberActivityClass);
            currentActivity.Call("startActivity", intent);
#endif

            return this;
        }
        public GetPhoneNumberHelper GetOTPMessage()
        {
            Debug.Log("Get OTP Invoked");
#if UNITY_ANDROID

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass otpActivityClass = new AndroidJavaClass("com.appinop.autoreadotp.ReadOTPActivity");

            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", currentActivity, otpActivityClass);
            currentActivity.Call("startActivity", intent);
#endif

            return this;
        }

        public GetPhoneNumberHelper OnSuccess(Action<string> onSuccessCallback)
        {
            PhoneNumberReceived = onSuccessCallback;
            return this;
        }

        public GetPhoneNumberHelper OnError(Action<string> onErrorCallback)
        {
            PhoneNumberError = onErrorCallback;
            return this;
        }


        private void onOTP(string data)
        {
            PhoneNumberReceived?.Invoke(data);
            DestroySelf();
        }
        private void onSuccess(string data)
        {
            PhoneNumberReceived?.Invoke(data);
            DestroySelf();
        }

        private void onFailure(string message)
        {
            PhoneNumberError?.Invoke(message);
            DestroySelf();
        }
        private void DestroySelf()
        {
            //Destroy(gameObject);  // Destroy the GameObject holding the GetPhoneNumberHelper
            //instance = null; // Reset the instance
        }
    }
}
