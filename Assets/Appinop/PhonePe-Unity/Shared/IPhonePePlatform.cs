using System;

namespace Appinop.PhonePePlugin
{

    public interface IPhonePePlatform
    {
        void InitializePhonePe(string environment, string merchantId, string appSchema);
        void StartTransaction(string base64Payload, string checksum);
    }

}