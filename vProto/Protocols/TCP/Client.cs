using System;
using System.Net;
using System.Net.Sockets;

namespace vProto.Protocols.TCP
{
    using Events;

    /// <summary>
    /// A TCP/IP client.
    /// </summary>
    public class Client
        : vProto.BaseClient
    {
        /// <summary>
        /// Underlying TcpClient object.
        /// </summary>
        protected TcpClient client;
        /// <summary>
        /// Underlying NetworkStream associated with the TcpClient.
        /// </summary>
        protected NetworkStream Nstream;
        /// <summary>
        /// IP endpoint of the server to which the client is connected.
        /// </summary>
        protected IPEndPoint server;



        /// <summary>
        /// Releases all the resources used by the current instance of vProto.Protocols.TCP.Client.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            try
            {
                client.Close();
            }
            catch (Exception)
            {

            }

            try
            {
                Nstream.Close();
            }
            catch (Exception)
            {

            }
        }



        /// <summary>
        /// Default constructor to be used by inheriting classes.
        /// </summary>
        /// <param name="server">Server object which owns this client.</param>
        protected Client(BaseServer server)
        {
            Owner = server;
        }



        internal Client(BaseServer server, TcpClient client)
        {
            Owner = server;

            this.client = client;
            Nstream = client.GetStream();

            HandleTcpServerConnectionSuccess();
        }



        /// <summary>
        /// Initializes a new instance of the vProto.Protocols.TCP.Client class targeted at the given server.
        /// </summary>
        /// <param name="server">The address of the server to which this client will connect.</param>
        public Client(IPEndPoint server)
        {
            Owner = null;

            client = new TcpClient();

            this.server = server;
        }



        /// <summary>
        /// Starts the connection to the server.
        /// </summary>
        protected override void StartConnection()
        {
            client.BeginConnect(server.Address, server.Port, ConnectionCallback, null);
        }


        void ConnectionCallback(IAsyncResult ar)
        {
            try
            {
                client.EndConnect(ar);
            }
            catch (Exception x)
            {
                OnConnectionFailed(new ClientConnectionFailedEventArgs(x));

                return;
            }

            if (client.Connected)
            {
                Nstream = client.GetStream();

                HandleTcpClientConnectionSuccess();
            }
        }



        /// <summary>
        /// Used by inheriting classes to do more with the freshly-acquired TCP/IP client.
        /// </summary>
        protected virtual void HandleTcpServerConnectionSuccess()
        {
            InitializeFromStreams(Nstream);
        }

        /// <summary>
        /// Used by inheriting classes to do more with the freshly-connected TCP/IP client.
        /// </summary>
        protected virtual void HandleTcpClientConnectionSuccess()
        {
            InitializeFromStreams(Nstream);
        }
    }
}
