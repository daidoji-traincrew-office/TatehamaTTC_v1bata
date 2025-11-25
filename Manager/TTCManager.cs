using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaTTC_v1bata.Manager
{
    using OpenIddict.Client;
    using TatehamaTTC_v1bata.Network;
    internal class TTCManager
    {
        Network Network;

        internal TTCManager(OpenIddictClientService service)
        {
            Network = new Network(service);
        }

        /// <summary>
        /// 認証開始
        /// </summary>
        internal void NetworkAuthorize()
        {
            Network.Authorize();
        }
    }
}
