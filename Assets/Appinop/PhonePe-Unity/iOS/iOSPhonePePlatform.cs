#if UNITY_EDITOR || UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Appinop.PhonePePlugin
{
    public class iOSPhonePePlatform : IPhonePePlatform
    {

        private string _merchantId;
        private string _appSchema;
        private string _environment;
        [DllImport("__Internal")]
        private static extern void makePGRequest(
            string environment,
            string merchantId,
            string base64Payload,
            string apiEndPoint,
            string checksum,
            string flowId,
            string appSchema,
            bool enableLogging
        );

        public void InitializePhonePe(string environment, string merchantId, string appSchema)
        {

            if (Enum.TryParse(environment, out PhonePeHelper.ENVIRONMENT env))
            {
                switch (env)
                {
                    case PhonePeHelper.ENVIRONMENT.SANDBOX:
                        this._environment = "sandbox";
                        break;
                    case PhonePeHelper.ENVIRONMENT.PRODUCTION:
                        this._environment = "production";
                        break;

                    default:
                        this._environment = "production";
                        break;

                }

            }
            this._merchantId = merchantId;
            this._appSchema = appSchema;
            this._environment = appSchema;
            Debug.Log("PhonePe initialized on iOS with environment: " + environment);
        }

        public void StartTransaction(string base64Payload, string checksum)
        {

            makePGRequest(_environment, _merchantId, base64Payload, "/pg/v1/pay", checksum, DateTime.Now.Ticks.ToString(), _appSchema, false);
            Debug.Log("Transaction started on iOS.");

        }
    }

}

#endif