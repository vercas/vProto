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
    /// <summary>
    /// Handles communication with a server.
    /// </summary>
    public class Client
        : BaseClient
    {
        internal static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers. 
            return false;
        }



        string srvName;



        /// <summary>
        /// Initializes a new instance of the vProto.Client class.
        /// </summary>
        /// <param name="srvname">optional; Server name for SSL certificate verification. Use null for no SSL.</param>
        public Client(string srvname = null)
        {
            srvName = srvname;
        }



        /// <summary>
        /// Starts an asynchronous request for a remote host connection.
        /// </summary>
        /// <param name="ep">The IP address and port of the remote host.</param>
        /// <returns>True if the connection was requested; otherwise false (if already connected or connection failed from the start)</returns>
        public bool Connect(IPEndPoint ep)
        {
            if (Disposed)
                throw new ObjectDisposedException(base.GetType().FullName, "Re-instantiate the client to reconnect!");

            if (IsConnected)
                return false;

            client = new TcpClient();

#if RECEIVER_THREAD
            receiver = new Thread(new ThreadStart(ReceiverLoop));
#endif

#if SENDER_THREAD
            sender = new Thread(new ThreadStart(SenderLoop));
#endif

            try
            {
                client.BeginConnect(ep.Address, ep.Port, ConnectionCallback, null);

                return true;
            }
            catch (Exception x)
            {
                OnConnectionFailed(new Events.ClientConnectionFailedEventArgs(x));

                return false;
            }
        }

        void ConnectionCallback(IAsyncResult ar)
        {
            try
            {
                client.EndConnect(ar);
            }
            catch (Exception x)
            {
                OnConnectionFailed(new Events.ClientConnectionFailedEventArgs(x));

                return;
            }
            
            if (client.Connected)
            {
                Nstream = client.GetStream();

                if (srvName == null)
                {
                    heartbeatTimer = new System.Threading.Timer(__heartbeatTimerCallback, null, HeartbeatInterval, HeartbeatInterval);

                    IsConnected = true;

                    stream = Nstream;

                    OnConnected(new EventArgs());

                    LowStartGettingPackets();

                    //Console.WriteLine("Destination: {0}", client.Client.RemoteEndPoint);
                }
                else
                {
                    Sstream = new SslStream(Nstream, false, ValidateServerCertificate, null);

                    try
                    {
                        Sstream.BeginAuthenticateAsClient(srvName, FinishAuthentication, null);
                    }
                    catch (Exception x)
                    {
                        IsConnected = false;

                        OnAuthFailed(new Events.ClientAuthFailedEventArgs(x));
                    }
                }
            }
        }

        void FinishAuthentication(IAsyncResult ar)
        {
            try
            {
                Sstream.EndAuthenticateAsClient(ar);
            }
            catch (Exception x)
            {
                IsConnected = false;

                OnAuthFailed(new Events.ClientAuthFailedEventArgs(x));

                return;
            }

            heartbeatTimer = new System.Threading.Timer(__heartbeatTimerCallback, null, new TimeSpan(0, 0, 1), HeartbeatInterval);

            IsConnected = true;

            stream = Sstream;

            OnConnected(new EventArgs());

            LowStartGettingPackets();

            //Console.WriteLine("Destination: {0}", client.Client.RemoteEndPoint);
        }
    }
}
