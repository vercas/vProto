using System;
using System.Net;
using System.Net.Sockets;

namespace vProto.Protocols.TCP
{
    using Events;

    /// <summary>
    /// A TCP/IP server.
    /// </summary>
    public class Server
        : BaseServer
    {
        TcpListener listener;



        /// <summary>
        /// Releases all the resources used by the current instance of vProto.Protocols.TCP.Server.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            try
            {
                listener.Stop();
            }
            catch (Exception)
            {

            }
        }



        /// <summary>
        /// Initializes a new instance of the vProto.Protocols.TCP.Server which will listen for connections on the specified port.
        /// </summary>
        /// <param name="port">The port on which to listen for connections.</param>
        public Server(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }



        /// <summary>
        /// Starts listenning for connections on the previously-specified TCP port.
        /// </summary>
        /// <returns></returns>
        protected override bool StartListening()
        {
            listener.Start();

            IsOn = true;

            OnStarted(new EventArgs());

            return StartAcceptingClients();
        }


        bool StartAcceptingClients()
        {
            try
            {
                listener.BeginAcceptTcpClient(AcceptClient, null);

                return true;
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);

                return false;
            }
            catch (SocketException x)
            {
                OnClientConnectionFailed(new ServerClientConnectionFailedEventArgs(/*-1, null,*/ x));

                if (x.SocketErrorCode == SocketError.Disconnecting || x.SocketErrorCode == SocketError.Fault
                    || x.SocketErrorCode == SocketError.NotConnected || x.SocketErrorCode == SocketError.NotInitialized
                    || x.SocketErrorCode == SocketError.NotSocket || x.SocketErrorCode == SocketError.OperationAborted
                    || x.SocketErrorCode == SocketError.Shutdown || x.SocketErrorCode == SocketError.SocketError)
                {
                    _CheckIfStopped(x);

                    return false;
                }
            }

            //  This will run in case of non-shutdown-worthy circumstances.
            //  My previous comments on this line are invalid.

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

                if (x.SocketErrorCode == SocketError.Disconnecting || x.SocketErrorCode == SocketError.Fault
                    || x.SocketErrorCode == SocketError.NotConnected || x.SocketErrorCode == SocketError.NotInitialized
                    || x.SocketErrorCode == SocketError.NotSocket || x.SocketErrorCode == SocketError.OperationAborted
                    || x.SocketErrorCode == SocketError.Shutdown || x.SocketErrorCode == SocketError.SocketError)
                {
                    _CheckIfStopped(x);
                }
                else
                {
                    //  Maybe this was just bad luck? :L
                    StartAcceptingClients();
                }

                return;
            }

            try
            {
                HandleTcpClient(c);
            }
            finally
            {
                //  FFS.
                StartAcceptingClients();
            }
        }



        /// <summary>
        /// Used by inheriting classes to do more with the received TCP client.
        /// </summary>
        protected virtual void HandleTcpClient(TcpClient client)
        {
            ClientReceived(new Client(this, client));
        }
    }
}
