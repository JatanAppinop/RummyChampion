using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Appinop.PhonePePlugin
{
    public class UnsupportedPhonePePlatform : IPhonePePlatform
    {
        public void InitializePhonePe(string environment, string merchantId, string appSchema)
        {
            Debug.LogError("PhonePe is not supported on this platform.");
        }

        public void StartTransaction(string base64Payload, string checksum)
        {
            Debug.LogError("PhonePe transactions are not supported on this platform.");
        }
    }
}
