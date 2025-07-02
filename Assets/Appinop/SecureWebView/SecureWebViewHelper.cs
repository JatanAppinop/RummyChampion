using System;
using UnityEngine;

namespace Appinop.SecureWebView
{
    public class SecureWebViewHelper : MonoBehaviour
    {
        private static SecureWebViewHelper instance;

        public static SecureWebViewHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SecureWebViewHelper>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<SecureWebViewHelper>();
                        singletonObject.name = "SecureWebView_Helper";
                    }
                }
                return instance;
            }

        }

        private const string ActivityClass = "com.appinop.securewebview.PaymentWebViewActivity";
        private event Action<string> ResponseReceived;

#if UNITY_ANDROID && !UNITY_EDITOR
        public SecureWebViewHelper OpenPaymentWebView(string paymentUrl)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass pluginClass = new AndroidJavaClass(ActivityClass);

                currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", currentActivity, pluginClass);
                    intent.Call<AndroidJavaObject>("putExtra", "PaymentURL", paymentUrl);
                    currentActivity.Call("startActivity", intent);
                }));
            }

            return this;
        }

        public SecureWebViewHelper OnResponseReceived(Action<string> onResponseCallback)
        {
            ResponseReceived = onResponseCallback;
            return this;
        }

        private void OnResponse(string data)
        {
            Debug.Log($"SecureWebViewHelper response  data is :-{data}");
            ResponseReceived?.Invoke(data);
            DestroySelf();
            
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
            instance = null;
        }
#endif
    }
}
