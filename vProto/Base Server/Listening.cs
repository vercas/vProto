using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace vProto
{
    using vProto.Events;

    partial class BaseServer
    {
        /// <summary>
        /// Starts listening to incomming connection requests asynchronously.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (IsOn)
                return false;

            try
            {
                return StartListening();
            }
            catch (Exception x)
            {
                OnServerStartupFailed(new ServerStartupFailedEventArgs(x));

                return false;
            }
        }
    }
}
