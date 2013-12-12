using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace vProto.Protocols.TCP.SSL
{
    using Events;

    /// <summary>
    /// A TCP/IP client.
    /// </summary>
    public sealed class Client
        : vProto.Protocols.TCP.Client
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



        SslStream Sstream;

        X509Certificate cert;
        string srvName;



        /// <summary>
        /// Releases all the resources used by the current instance of vProto.ClientHandler.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            try
            {
                Sstream.Close();
            }
            catch (Exception)
            {

            }
        }



        internal Client(TcpClient client, X509Certificate certificate)
            : base()
        {
            this.client = client;
            Nstream = client.GetStream();

            cert = certificate;

            Sstream = new SslStream(Nstream, false);

            try
            {
                Sstream.BeginAuthenticateAsServer(certificate, false, SslProtocols.Default, true, FinishServerAuthentication, null);
            }
            catch (Exception x)
            {
                OnAuthFailed(new ClientAuthFailedEventArgs(x));

                _CheckIfStopped(x, true);
            }
        }



        /// <summary>
        /// Initializes a new instance of the vProto.Protocols.TCP.Client class targeted at the given server.
        /// </summary>
        /// <param name="server">The address of the server to which this client will connect.</param>
        /// <param name="serverName">The name of the server on its certificate.</param>
        public Client(IPEndPoint server, string serverName)
            : base(server)
        {
            srvName = serverName;
        }



        protected override void HandleTcpClientConnectionSuccess()
        {
            Sstream = new SslStream(Nstream, false, ValidateServerCertificate, null);

            try
            {
                Sstream.BeginAuthenticateAsClient(srvName, FinishClientAuthentication, null);
            }
            catch (Exception x)
            {
                OnAuthFailed(new ClientAuthFailedEventArgs(x));

                _CheckIfStopped(x, true);
            }
        }



        void FinishServerAuthentication(IAsyncResult ar)
        {
            try
            {
                Sstream.EndAuthenticateAsServer(ar);
            }
            catch (Exception x)
            {
                OnAuthFailed(new ClientAuthFailedEventArgs(x));

                _CheckIfStopped(x, true);

                return;
            }

            InitializeFromStreams(Sstream);
        }

        void FinishClientAuthentication(IAsyncResult ar)
        {
            try
            {
                Sstream.EndAuthenticateAsServer(ar);
            }
            catch (Exception x)
            {
                OnAuthFailed(new ClientAuthFailedEventArgs(x));

                _CheckIfStopped(x, true);

                return;
            }

            InitializeFromStreams(Sstream);
        }

        //  These are kept separate for now for possible future use.
    }
}
