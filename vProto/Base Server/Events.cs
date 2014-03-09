using System;

namespace vProto
{
    using Events;

    partial class BaseServer
    {
        /// <summary>
        /// Runs when the server successfully started.
        /// </summary>
        public event ServerEventHandler Started;
        /// <summary>
        /// Runs when the server failed to start.
        /// </summary>
        public event ServerEventHandler<ServerStartupFailedEventArgs> StartupFailed;
        /// <summary>
        /// Runs when the server stopped.
        /// </summary>
        public event ServerEventHandler<ServerStoppedEventArgs> Stopped;

        /// <summary>
        /// Runs when a client successfully connected to the server.
        /// </summary>
        public event ServerEventHandler<ServerClientConnectedEventArgs> ClientConnected;
        /// <summary>
        /// Runs when a client faile to connect to (or handshake with) the server.
        /// </summary>
        public event ServerEventHandler<ServerClientConnectionFailedEventArgs> ClientConnectionFailed;
        /// <summary>
        /// Runs when a client disconnected from the server.
        /// </summary>
        public event ServerEventHandler<ServerClientDisconnectedEventArgs> ClientDisconnected;



        /// <summary>
        /// Raises the Started event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnStarted(EventArgs e)
        {
            ServerEventHandler handler = Started;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the StartupFailed event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnStartupFailed(ServerStartupFailedEventArgs e)
        {
            ServerEventHandler<ServerStartupFailedEventArgs> handler = StartupFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the Stopped event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnStopped(ServerStoppedEventArgs e)
        {
            ServerEventHandler<ServerStoppedEventArgs> handler = Stopped;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        /// <summary>
        /// Raises the ClientConnected event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnClientConnected(ServerClientConnectedEventArgs e)
        {
            ServerEventHandler<ServerClientConnectedEventArgs> handler = ClientConnected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the ClientConnectionFailed event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnClientConnectionFailed(ServerClientConnectionFailedEventArgs e)
        {
            ServerEventHandler<ServerClientConnectionFailedEventArgs> handler = ClientConnectionFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the ClientDisconnected event.
        /// </summary>
        /// <param name="e">Desired event arguments.</param>
        protected virtual void OnClientDisconnected(ServerClientDisconnectedEventArgs e)
        {
            ServerEventHandler<ServerClientDisconnectedEventArgs> handler = ClientDisconnected;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
