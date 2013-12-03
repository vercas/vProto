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

    partial class Server
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
                listener.Start();

                IsOn = true;

                OnServerStarted(new EventArgs());
            }
            catch (SocketException x)
            {
                OnServerStartupFailed(new ServerStartupFailedEventArgs(x));

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

                OnClientConnectionFailed(new ServerClientConnectionFailedEventArgs(/*-1, null,*/ x));
            }

            //  No idea why I added this.
            //  But this line of code will never be reached.
            //  (there are returns in every block of the try/catch statement)

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
                OnClientConnectionFailed(new ServerClientConnectionFailedEventArgs(/*-1, null,*/ x));

                //  Maybe this was just bad luck? :L
                StartAcceptingClients();

                return;
            }

            try
            {
                var h = new ClientHandler(c, cert);

                _AddClient(h);

                OnClientConnected(new ServerClientConnectedEventArgs(h));
            }
            finally
            {
                //  FFS.
                StartAcceptingClients();
            }
        }
    }
}
