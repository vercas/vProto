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
using System.Threading;

namespace vProto
{
    using Internal_Utilities;
    using Events;
    using Packages;

    partial class BaseClient
    {
        /// <summary>
        /// Starts an asynchronous request for a remote host connection.
        /// </summary>
        public bool Connect()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Re-instantiate the client to reconnect!");

            if (IsClientHandler)
                throw new InvalidOperationException("This object handles a server's connection to a client. This method call is unsupported.");

            if (IsConnected)
                return false;

            try
            {
                StartConnection();

                return true;
            }
            catch (Exception x)
            {
                OnConnectionFailed(new Events.ClientConnectionFailedEventArgs(x));

                return false;
            }
        }
    }
}
