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

    /// <summary>
    /// Listens and handles connections.
    /// </summary>
    public partial class Server
        : IDisposable
    {
        TcpListener listener;

        X509Certificate cert;



        /// <summary>
        /// Gets a value indicating whether the object is disposed or not.
        /// </summary>
        public Boolean Disposed { get; private set; }

        /// <summary>
        /// Releases all the resources used by the current instance of vProto.Server.
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _chs.Length; i++)
                if (_chs[i] != null && !_chs[i].Disposed)
                    _chs[i].Dispose();

            try
            {
                listener.Stop();
            }
            catch (Exception)
            {

            }

            Disposed = true;
            IsOn = false;
        }



        public Boolean IsOn { get; private set; }

        private void _CheckIfStopped(Exception x)
        {
            if (IsOn)
            {
                Dispose();

                OnServerStopped(new ServerStoppedEventArgs(x));
            }
        }



        /// <summary>
        /// Initializes a new instance of the vProto.Server class.
        /// </summary>
        /// <param name="port">The port on which to listen for incomming connections.</param>
        /// <param name="cert">optional; The X.509 certificate to use for SSL authentication. Use null for no SSL.</param>
        public Server(int port, X509Certificate cert = null)
        {
            listener = new TcpListener(IPAddress.Any, port);
            this.cert = cert;
        }
    }
}
