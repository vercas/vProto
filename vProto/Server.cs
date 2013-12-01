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
    public class Server
        : IDisposable
    {
        #region IDisposable Implementation
        public Boolean IsDisposed { get; private set; }

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

            IsDisposed = true;
            IsOn = false;
        }
        #endregion


        public event EventHandler ServerStarted;
        public event EventHandler<Events.ServerStartupFailedEventArgs> ServerStartupFailed;
        public event EventHandler<Events.ServerStoppedEventArgs> ServerStopped;

        public event EventHandler<Events.ServerClientConnectedEventArgs> ClientConnected;
        public event EventHandler<Events.ClientConnectionFailedEventArgs> ClientConnectionFailed;


        #region Client management
        internal ClientHandler[] _chs = new ClientHandler[10];
        object _chs_sync = new object();

        private void _DoubleClientContainer()
        {
            lock (_chs_sync)
            {
                ClientHandler[] n = new ClientHandler[_chs.Length * 2];

                _chs.CopyTo(n, 0);

                _chs = n;
            }
        }

        private int _AddClient(ClientHandler h)
        {
            int pos = -1;

            lock (_chs_sync)
                for (int i = 0; i < _chs.Length; i++)
                    if (_chs[i] == null)
                    {
                        pos = i;
                        break;
                    }

            if (pos == -1)
            {
                _DoubleClientContainer();

                return _AddClient(h);
            }
            else
            {
                lock (_chs_sync)
                    _chs[pos] = h;

                h.Disconnected += h_Disconnected;

                return h.ID = pos;
            }
        }

        private void _RemoveClient(ClientHandler h)
        {
            lock (_chs_sync)
                if (h.ID > -1 && h.ID < _chs.Length)
                {
                    _chs[h.ID] = null;
                }
        }
        #endregion


        public Boolean IsOn { get; private set; }

        private void _CheckIfStopped(Exception x)
        {
            if (IsOn)
            {
                IsOn = false;

                /*try
                {
                    listener.Stop();
                }
                finally
                {
                    if (ServerStopped != null)
                        ServerStopped(this, new Events.ServerStoppedEventArgs(x));
                }*/

                Dispose();

                if (ServerStopped != null)
                    ServerStopped(this, new Events.ServerStoppedEventArgs(x));
            }
        }


        TcpListener listener;
        X509Certificate cert;


        public Server(int port, X509Certificate cert = null)
        {
            listener = new TcpListener(IPAddress.Any, port);
            this.cert = cert;
        }


        public bool Start()
        {
            if (IsOn)
                return false;

            try
            {
                listener.Start();

                IsOn = true;

                if (ServerStarted != null)
                    ServerStarted(this, new EventArgs());
            }
            catch (SocketException x)
            {
                if (ServerStartupFailed != null)
                    ServerStartupFailed(this, new Events.ServerStartupFailedEventArgs(x));

                return false;
            }
            catch
            {
                throw;
            }

            //heartbeatTimer = new System.Threading.Timer(new System.Threading.TimerCallback(__heartbeatTimerCallback), null, new TimeSpan(0, 0, 1), HeartbeatInterval);

            return StartAcceptingClients();
        }


        bool StartAcceptingClients()
        {
            try
            {
                listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClient), null);

                return true;
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);

                return false;
            }
            catch (SocketException x)
            {
                if (x.SocketErrorCode == SocketError.Disconnecting || x.SocketErrorCode == SocketError.Fault
                    || x.SocketErrorCode == SocketError.NotConnected || x.SocketErrorCode == SocketError.NotInitialized
                    || x.SocketErrorCode == SocketError.NotSocket || x.SocketErrorCode == SocketError.OperationAborted
                    || x.SocketErrorCode == SocketError.Shutdown || x.SocketErrorCode == SocketError.SocketError)
                {
                    _CheckIfStopped(x);

                    return false;
                }

                if (ClientConnectionFailed != null)
                    ClientConnectionFailed(this, new Events.ClientConnectionFailedEventArgs(x));
            }

            return StartAcceptingClients();
        }

        void AcceptClient(IAsyncResult ar)
        {
            TcpClient c;

            try
            {
                c = listener.EndAcceptTcpClient(ar);
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);

                return;
            }
            catch (SocketException x)
            {
                if (ClientConnectionFailed != null)
                    ClientConnectionFailed(this, new Events.ClientConnectionFailedEventArgs(x));

                //  Maybe this was just bad luck? :L
                StartAcceptingClients();

                return;
            }

            try
            {
                var h = new ClientHandler(c, cert);

                _AddClient(h);

                if (ClientConnected != null)
                    ClientConnected(this, new Events.ServerClientConnectedEventArgs(h.ID, h));
            }
            finally
            {
                //  FFS.
                StartAcceptingClients();
            }
        }



        /* Events from clients...
         */

        void h_Disconnected(BaseClient sender, Events.ClientDisconnectedEventArgs e)
        {
            _RemoveClient(sender as ClientHandler);
        }
    }
}
