using System;
using UnityEngine;

namespace Appinop.PhonePePlugin
{
    public class PhonePeHelper : MonoBehaviour
    {

        public enum ENVIRONMENT
        {
            SANDBOX,
            PRODUCTION
        }

        private static PhonePeHelper instance;
        private IPhonePePlatform platform;
        private Action<string> dataReceivedCallback;
        public static PhonePeHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PhonePeHelper>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<PhonePeHelper>();
                        singletonObject.name = "PhonePeManager";
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID
            platform = new AndroidPhonePePlatform();
#elif UNITY_IOS
        platform = new iOSPhonePePlatform();
#else
            platform = new UnsupportedPhonePePlatform();
#endif
        }

        public void InitializePhonePe(ENVIRONMENT environment, string merchantId, string appSchema)
        {
            platform.InitializePhonePe(environment.ToString(), merchantId, appSchema);
        }

        public void StartTransaction(string base64Payload, string checksum, Action<string> onResult)
        {
            dataReceivedCallback = onResult;
            platform.StartTransaction(base64Payload, checksum);
        }

        private void OnTransactionResult(string result)
        {
            Debug.Log("Transaction Result: " + result);
            dataReceivedCallback?.Invoke(result);
        }

    }
}

