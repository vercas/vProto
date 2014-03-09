using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

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
            using (var str1 = new MemoryStream())
            using (var bw = new BinaryWriter(str1, Encoding.UTF8))
            {
                bw.Write(ProtocolVersion.Major);
                bw.Write(ProtocolVersion.Minor);

                bw.Flush();

                __createInternalRequest(InternalRequestType.Handshake).AddFailureHandler((client, sender, e) =>
                {
                    OnConnectionFailed(new Events.ClientConnectionFailedEventArgs(e.Exception));
                }).SetPayload(str1).AddResponseReceivedHandler((client, sender, e) =>
                {
                    using (var str2 = new MemoryStream(e.Payload))
                    using (var br = new BinaryReader(str2, Encoding.UTF8))
                    {
                        int vma = br.ReadInt32();
                        int vmi = br.ReadInt32();

                        if (ProtocolVersion.Major != vma || ProtocolVersion.Minor != vmi)
                            _CheckIfStopped(new NotSupportedException("vProto Protocol Version mismatch."));
                        else
                            _Handshake2();
                    }
                }).SendFluent();
            }
        }

        private void _Handshake2()
        {
            using (var str = new MemoryStream())
            using (var bw = new BinaryWriter(str, Encoding.UTF8))
            {
                bw.Write(this._id);

                if (this._peers == null)
                    bw.Write(-1);
                else
                {
                    bw.Write(this._peers.Count);

                    for (int i = this._peers.Count - 1; i >= 0; i--)
                        bw.Write(this._peers[i]);
                }

                bw.Flush();

                __createInternalRequest(InternalRequestType.Handshake).AddFailureHandler((client, sender, e) =>
                {
                    OnConnectionFailed(new Events.ClientConnectionFailedEventArgs(e.Exception));
                }).SetPayload(str).AddResponseReceivedHandler((client, sender, e) =>
                {
                    _FinishHandhake();
                }).SendFluent();
            }
        }



        private void _handleHandshakeRequest(BaseClient sender, RequestReceivedEventArgs e)
        {
            if (handshakeStep == 0)
                using (var br = new BinaryReader(e.Response.RequestPayloadStream, Encoding.UTF8))
                {
                    int vma = br.ReadInt32();
                    int vmi = br.ReadInt32();

                    if (ProtocolVersion.Major != vma || ProtocolVersion.Minor != vmi)
                        _CheckIfStopped(new NotSupportedException("vProto Protocol Version mismatch."));
                    else
                    {
                        handshakeStep = 1;

                        using (var str = new MemoryStream())
                        using (BinaryWriter bw = new BinaryWriter(str, Encoding.UTF8))
                        {
                            bw.Write(ProtocolVersion.Major);
                            bw.Write(ProtocolVersion.Minor);

                            bw.Flush();

                            e.Response.SetPayload(str).Send();
                        }
                    }
                }
            else if (handshakeStep == 1)
                using (var br = new BinaryReader(e.Response.RequestPayloadStream, Encoding.UTF8))
                {
                    this._id = br.ReadInt32();

                    int peers_cnt = br.ReadInt32();
                    List<int> peers = null;

                    if (peers_cnt != -1)
                    {
                        peers = new List<int>(peers_cnt);

                        for (int i = peers_cnt - 1; i >= 0; i--)
                            peers[i] = br.ReadInt32();
                    }

                    lock (_peers_lock)
                    {
                        _peers = peers;

                        if (peers != null)
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

            //Console.WriteLine("Handshake finished! ({0})", _id);
        }
    }
}
