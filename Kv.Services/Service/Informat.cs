using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace KvServices.Service
{
    internal class Informat: IInformat
    {
        private IServerAddressesFeature ServerAddressesFeature { get; }

        public Informat(IServerAddressesFeature serverAddressesFeature)
        {
            ServerAddressesFeature = serverAddressesFeature;
        }

        public string ServiceUrl
        {
            get
            {
                try
                {
                    return ServerAddressesFeature.Addresses.FirstOrDefault();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
}
