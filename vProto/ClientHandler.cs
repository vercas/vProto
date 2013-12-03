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
    public class ClientHandler
        : BaseClient
    {
        //


        public Int32 ID { get; internal set; }


        internal ClientHandler(TcpClient client, X509Certificate cert)
        {
            ID = -1;
            this.client = client;
            Nstream = client.GetStream();

#if RECEIVER_THREAD
            receiver = new Thread(new ThreadStart(ReceiverLoop));
#endif

#if SENDER_THREAD
            sender = new Thread(new ThreadStart(SenderLoop));
#endif

            if (cert == null)
            {
                heartbeatTimer = new System.Threading.Timer(new System.Threading.TimerCallback(__heartbeatTimerCallback), null, HeartbeatInterval, HeartbeatInterval);

                IsConnected = true;

                stream = Nstream;

                OnConnected(new EventArgs());

                LowStartGettingPackets();
            }
            else
            {
                Sstream = new SslStream(Nstream, false);

                try
                {
                    Sstream.BeginAuthenticateAsServer(cert, false, SslProtocols.Default, true, new AsyncCallback(FinishAuthentication), null);
                    //stream.AuthenticateAsServer(cert, false, SslProtocols.Default, true);
                }
                catch (Exception x)
                {
                    OnAuthFailed(new Events.ClientAuthFailedEventArgs(x));

                    throw x;
                }
            }
        }

        void FinishAuthentication(IAsyncResult ar)
        {
            try
            {
                Sstream.EndAuthenticateAsServer(ar);
            }
            catch (Exception x)
            {
                OnAuthFailed(new Events.ClientAuthFailedEventArgs(x));

                throw x;
            }

            heartbeatTimer = new System.Threading.Timer(new System.Threading.TimerCallback(__heartbeatTimerCallback), null, HeartbeatInterval, HeartbeatInterval);

            IsConnected = true;

            stream = Sstream;

            OnConnected(new EventArgs());

            LowStartGettingPackets();
        }
    }
}
