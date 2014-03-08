using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace vProto
{
    using Events;
    using Internals;
    using Packages;

    partial class BaseClient
    {
        int handshakeStep = 0;



        private void _StartHandshake()
        {
            __createInternalRequest(InternalRequestType.Handshake).AddFailureHandler((client1, sender1, e1) =>
            {
                OnConnectionFailed(new Events.ClientConnectionFailedEventArgs(e1.Exception));
            }).SetPayload(BinarySerialization.Serialize(new Handshake1Req
            {
                ProtocolVersion = ProtocolVersion
            })).AddResponseReceivedHandler((client1, sender1, e1) =>
            {
                var data1 = BinarySerialization.Deserialize<Handshake1Res>(e1.Payload);

                if (data1.ProtocolVersion != ProtocolVersion)
                    this.Dispose();
                else
                {
                    __createInternalRequest(InternalRequestType.Handshake).AddFailureHandler((client2, sender2, e2) =>
                    {
                        OnConnectionFailed(new Events.ClientConnectionFailedEventArgs(e2.Exception));
                    }).SetPayload(BinarySerialization.Serialize(new Handshake2Req
                    {
                        PeerIDs = _peers,
                        ClientID = this._id
                    })).AddResponseReceivedHandler((client2, sender2, e2) =>
                    {
                        _FinishHandhake();
                    }).SendFluent();
                }
            }).SendFluent();
        }

        private void _handleHandshakeRequest(BaseClient sender, RequestReceivedEventArgs e)
        {
            if (handshakeStep == 0)
            {
                handshakeStep = 1;

                var data = BinarySerialization.Deserialize<Handshake1Req>(e.Response.RequestPayload);

                e.Response.SetPayload(BinarySerialization.Serialize(new Handshake1Res
                {
                    ProtocolVersion = ProtocolVersion
                })).Send();
            }
            else if (handshakeStep == 1)
            {
                var data = BinarySerialization.Deserialize<Handshake2Req>(e.Response.RequestPayload);

                _id = data.ClientID;

                lock (_peers_lock)
                {
                    _peers = data.PeerIDs;

                    if (_peers != null)
                        for (int i = 0; i < _peers_queued_temp.Count; i++)
                            if (!_peers.Contains(_peers_queued_temp[i]))
                                _peers.Add(_peers_queued_temp[i]);

                    _peers_queued_temp = null;
                }

                _FinishHandhake();

                e.Response.Send();
            }
        }

        private void _FinishHandhake()
        {
            IsConnected = true;

            OnConnected(new EventArgs());

            Console.WriteLine("Handshake finished! ({0})", _id);
        }
    }



    //  Handshake data containers



    [Serializable]
    internal class Handshake1Req
    {
        internal Version ProtocolVersion;
    }

    [Serializable]
    internal class Handshake1Res
    {
        internal Version ProtocolVersion;
    }


    [Serializable]
    internal class Handshake2Req
    {
        [OptionalField]
        internal List<int> PeerIDs;
        internal int ClientID;
    }
}
