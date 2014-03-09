using System;

namespace vProto
{
    using Events;

    partial class BaseClient
    {
        /// <summary>
        /// Runs when a full connection (including handshake) occurs successfully.
        /// </summary>
        public event ClientEventHandler Connected;
        /// <summary>
        /// Runs when a connection or handshake failed.
        /// </summary>
        public event ClientEventHandler<ClientConnectionFailedEventArgs> ConnectionFailed;
        /// <summary>
        /// Runs when authentication (if any) failed.
        /// </summary>
        public event ClientEventHandler<ClientAuthFailedEventArgs> AuthFailed;
        /// <summary>
        /// Runs when the client is disconnected (either voluntarily or not).
        /// </summary>
        public event ClientEventHandler<ClientDisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Runs when a peer connection is signalled by the server.
        /// </summary>
        public event ClientEventHandler<PeerChangeEventArgs> PeerConnected;
        /// <summary>
        /// Runs when a peer disconnection is signalled by the server.
        /// </summary>
        public event ClientEventHandler<PeerChangeEventArgs> PeerDisconnected;

        /// <summary>
        /// Runs when a data package is received from the other side.
        /// </summary>
        public event ClientEventHandler<DataReceivedEventArgs> DataReceived;
        /// <summary>
        /// Runs when a request is received from the other side.
        /// </summary>
        public event ClientEventHandler<RequestReceivedEventArgs> RequestReceived;

        /// <summary>
        /// Runs when an error occured while attempting to send data.
        /// </summary>
        public event ClientEventHandler<PipeFailureEventArgs> SendFailed;
        /// <summary>
        /// Runs when an error occured while attempting to receive data.
        /// </summary>
        public event ClientEventHandler<PipeFailureEventArgs> ReceiptFailed;


        /// <summary>
        /// Raises the Connected event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnConnected(EventArgs e)
        {
            ClientEventHandler handler = Connected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the ConnectionFailed event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnConnectionFailed(ClientConnectionFailedEventArgs e)
        {
            ClientEventHandler<ClientConnectionFailedEventArgs> handler = ConnectionFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the AuthFailed event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnAuthFailed(ClientAuthFailedEventArgs e)
        {
            ClientEventHandler<ClientAuthFailedEventArgs> handler = AuthFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the Disconnected event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            ClientEventHandler<ClientDisconnectedEventArgs> handler = Disconnected;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        /// <summary>
        /// Raises the PeerConnected event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnPeerConnected(PeerChangeEventArgs e)
        {
            ClientEventHandler<PeerChangeEventArgs> handler = PeerConnected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the PeerDisconnected event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnPeerDisconnected(PeerChangeEventArgs e)
        {
            ClientEventHandler<PeerChangeEventArgs> handler = PeerDisconnected;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        /// <summary>
        /// Raises the DataReceived event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            ClientEventHandler<DataReceivedEventArgs> handler = DataReceived;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the RequestReceived event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnRequestReceived(RequestReceivedEventArgs e)
        {
            ClientEventHandler<RequestReceivedEventArgs> handler = RequestReceived;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        /// <summary>
        /// Raises the PipeFailure event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnPipeFailure(PipeFailureEventArgs e)
        {
            ClientEventHandler<PipeFailureEventArgs> handler;

            if (e.Outgoing)
                handler = SendFailed;
            else
                handler = ReceiptFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
