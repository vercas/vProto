using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto
{
    using Internal_Utilities;
    using Events;
    using Packages;

    partial class BaseClient
    {
        public event ClientEventHandler Connected;
        public event ClientEventHandler<ClientConnectionFailedEventArgs> ConnectionFailed;
        public event ClientEventHandler<ClientAuthFailedEventArgs> AuthFailed;
        public event ClientEventHandler<ClientDisconnectedEventArgs> Disconnected;

        public event ClientEventHandler<DataReceivedEventArgs> DataReceived;
        public event ClientEventHandler<RequestReceivedEventArgs> RequestReceived;

        public event ClientEventHandler<PipeFailureEventArgs> SendFailed;
        public event ClientEventHandler<PipeFailureEventArgs> ReceiptFailed;


        protected virtual void OnConnected(EventArgs e)
        {
            ClientEventHandler handler = Connected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnConnectionFailed(ClientConnectionFailedEventArgs e)
        {
            ClientEventHandler<ClientConnectionFailedEventArgs> handler = ConnectionFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAuthFailed(ClientAuthFailedEventArgs e)
        {
            ClientEventHandler<ClientAuthFailedEventArgs> handler = AuthFailed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            ClientEventHandler<ClientDisconnectedEventArgs> handler = Disconnected;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        protected virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            ClientEventHandler<DataReceivedEventArgs> handler = DataReceived;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRequestReceived(RequestReceivedEventArgs e)
        {
            ClientEventHandler<RequestReceivedEventArgs> handler = RequestReceived;

            if (handler != null)
            {
                handler(this, e);
            }
        }


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
