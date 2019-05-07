using System;
using System.IO;
using System.Security.Cryptography;

namespace KvServices.Repository
{
    internal class CalcHashcode : ICalcHashcode
    {
        public string CacleBytes(byte[] bytes)
        {
            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                using (var memoryStream = new MemoryStream(bytes))
                {

                    byte[] buffer = md5Provider.ComputeHash(memoryStream);

                    md5Provider.Clear();

                    string resule = BitConverter.ToString(buffer);
                    return resule.Replace("-", "");
                }
            }
        }
    }
}
