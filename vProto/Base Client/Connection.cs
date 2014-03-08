using System;

namespace vProto
{
    using Events;

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

            if (IsInternallyConnected)
                return false;

            try
            {
                StartConnection();

                return true;
            }
            catch (Exception x)
            {
                OnConnectionFailed(new ClientConnectionFailedEventArgs(x));

                return false;
            }
        }
    }
}
