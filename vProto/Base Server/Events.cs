using System;

namespace vProto
{
    using Events;

    partial class BaseServer
    {
        public event ServerEventHandler ServerStarted;
        public event ServerEventHandler<ServerStartupFailedEventArgs> ServerStartupFailed;
        public event ServerEventHandler<ServerStoppedEventArgs> ServerStopped;

        public event ServerEventHandler<ServerClientConnectedEventArgs> ClientConnected;
        public event ServerEventHandler<ServerClientConnectionFailedEventArgs> ClientConnectionFailed;
        public event ServerEventHandler<ServerClientDisconnectedEventArgs> ClientDisconnected;



        protected virtual void OnServerStarted(EventArgs e)
        {
            ServerEventHandler handler = ServerStarted;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnServerStartupFailed(ServerStartupFailedEventArgs e)
        {
            ServerEventHandler<ServerStartupFailedEventArgs> handler = ServerStartupFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnServerStopped(ServerStoppedEventArgs e)
        {
            ServerEventHandler<ServerStoppedEventArgs> handler = ServerStopped;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        protected virtual void OnClientConnected(ServerClientConnectedEventArgs e)
        {
            ServerEventHandler<ServerClientConnectedEventArgs> handler = ClientConnected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnClientConnectionFailed(ServerClientConnectionFailedEventArgs e)
        {
            ServerEventHandler<ServerClientConnectionFailedEventArgs> handler = ClientConnectionFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

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
