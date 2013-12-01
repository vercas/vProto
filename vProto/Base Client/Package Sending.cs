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
    using Internal_Utilities;
    using Events;
    using Packages;

    /*using StateType = Packages.Package;
    using QueuedPackageType = Internal_Utilities.QueuedPackage;*/

    partial class BaseClient
    {
        bool LowSendPacket(PackageHeader header, byte[] bodeh, Action cbk = null, object state = null)
        {
            if (Disposed)
                throw new ObjectDisposedException("vProto.BaseClient", "This client base is disposed of!");

            if (!sender.IsAlive)
                sender.Start();
            
            header.Size = (uint)bodeh.Length;

            lock (send_lock)
            {
                sendingBuffer.Write(Struct_mapping.StructureToByteArray(header), 0, packetHeaderSize);
                sendingBuffer.Write(bodeh, 0, bodeh.Length);

                var st = new Package(header, bodeh) { State = state };

                if (cbk != null)
                    st.callbacks.Add(cbk);

                toSend.Enqueue(st);
            }

            return true;
        }



        private object send_lock = new object();

        Queue<Package> toSend = new Queue<Package>();

        System.IO.MemoryStream sendingBuffer = new System.IO.MemoryStream();
        System.IO.MemoryStream tempBuffer = new System.IO.MemoryStream();

        //  Swapped buffers. Brilliant idea.

        protected void SenderLoop()
        {
            Queue<Package> tempQ = new Queue<Package>();

            //System.IO.MemoryStream tempB = new System.IO.MemoryStream();
            System.IO.MemoryStream temp;

            while (!Disposed)
            {
                tempQ.Clear();

                //tempB.Seek(0, System.IO.SeekOrigin.Begin);
                //tempB.SetLength(0);

                lock (send_lock)
                {
                    while (toSend.Count > 0)
                        tempQ.Enqueue(toSend.Dequeue());

                    //sendingBuffer.CopyTo(tempB);

                    temp = sendingBuffer;
                    sendingBuffer = tempBuffer;
                    tempBuffer = temp;

                    //  Emptying the new sending buffa'!
                    sendingBuffer.Seek(0, System.IO.SeekOrigin.Begin);
                    sendingBuffer.SetLength(0);
                }

                if (tempQ.Count > 0)
                {
                    Console.WriteLine("Sending {0} packages, totalling in {1} bytes!", tempQ.Count, tempBuffer.Length);

                    tempBuffer.Seek(0, System.IO.SeekOrigin.Begin);

                    try
                    {
                        tempBuffer.CopyTo(stream, (int)Math.Min(tempBuffer.Length, int.MaxValue));
                        //  Really making sure here.
                    }
                    catch (ObjectDisposedException x)
                    {
                        _CheckIfStopped(x);

                        return;
                    }
                    catch (Exception x)
                    {
                        foreach (var pack in tempQ)
                            _OnPipeFailure(x, true, pack);

                        return;
                    }

                    foreach (var pack in tempQ)
                    {
                        foreach (var cbk in pack.callbacks)
                            cbk();

                        OnInternalPacketSent(pack);
                    }
                }

                if (!Thread.Yield())
                    Thread.Sleep(10);

                //  If unable to yield, sleep for 1/100 seconds.
                //  This shall not do any harm.
            }
        }


        /// <summary>
        /// Gets a value indicating whether a packet is being sent.
        /// <para>Attempting to send anything else while this value is true will add the package to a queue, which will be sent as soon as the pipe is free.</para>
        /// </summary>
        public Boolean IsSendingPacket { get { return false; } }


        /*private object send_lock = new object();
        private bool sending = false;

        Queue<QueuedPackageType> packageQueue = new Queue<QueuedPackageType>();

        bool LowSendPacket(PackageHeader header, byte[] bodeh, Action cbk = null, object state = null)
        {
            var len = bodeh.Length + packetHeaderSize;
            byte[] pack = new byte[len];

            header.Size = (uint)bodeh.Length;

            Struct_mapping.StructureToByteArray(header).CopyTo(pack, 0);
            bodeh.CopyTo(pack, packetHeaderSize);

            var st = new StateType(header, bodeh);

            st.State = state;

            if (cbk != null)
                st.callbacks.Add(cbk);

            return __sendPack(pack, LowSendPacket_Callback, st);
        }

        void LowSendPacket_Callback(IAsyncResult ar)
        {
            try
            {
                lock (send_lock)
                {
                    stream.EndWrite(ar);

                    sending = packageQueue.Count > 0;
                }
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);

                return;
            }
            catch (Exception x)
            {
                _OnPipeFailure(x, true, ar.AsyncState as StateType);

                return;
            }

            var state = ar.AsyncState as StateType;

            try
            {
                if (state != null)
                {
                    foreach (var cbk in state.callbacks)
                        cbk();

                    OnInternalPacketSent(state);
                }
                //else dafuq?
            }
            finally //  The world goes on without you. :(
            {
                QueuedPackageType t = null;

                lock (send_lock)
                    if (packageQueue.Count > 0)
                        t = packageQueue.Dequeue();

                if (t != null)
                    __sendPack(t.payload, t.callback, t.state, true);
            }
        }


        bool __sendPack(byte[] pack, AsyncCallback cbk, StateType st, bool force = false)
        {
            try
            {
                lock (send_lock)
                {
                    //  If we're sending and we're not being forced to send...
                    if (!force && sending)
                    {
                        packageQueue.Enqueue(new QueuedPackageType(pack, cbk, st));
                    }
                    else
                    {
                        stream.BeginWrite(pack, 0, pack.Length, cbk, st);
                        sending = true;
                    }
                }

                return true;
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);

                return false;
            }
            catch (Exception x)
            {
                _OnPipeFailure(x, true, st);

                return false;
            }
        }*/


        //  These functions also check their arguments.

        internal protected void _SendPack(byte[] payload, PackageHeader header, Action cbk = null, object state = null)
        {
            if (payload == null)
                throw new ArgumentNullException("payload", "Payload array cannot be null.");

            LowSendPacket(header, payload, cbk, state);
        }

        internal protected void _SendPack(System.IO.Stream payload, PackageHeader header, int? len = null, Action cbk = null, object state = null)
        {
            if (payload == null)
                throw new ArgumentNullException("payload", "Payload stream cannot be null.");

            byte[] buff = null;
            long buflen = -1;

            try
            {
                buflen = payload.Length;
            }
            catch (NotSupportedException x)
            {
                throw new ArgumentException("Unable to get the length of the payload stream.", "payload", x);
            }
            catch (ObjectDisposedException x)
            {
                throw new ArgumentException("Payload stream is disposed.", "payload", x);
            }

            if (len == null)
            {
                try
                {
                    payload.Seek(0, System.IO.SeekOrigin.Begin);
                }
                catch (NotSupportedException x)
                {
                    throw new ArgumentException("Unable to seek to the beginning of the payload stream.", "payload", x);
                }
            }
            else if (len >= 0)
            {
                try
                {
                    buflen -= payload.Position;
                }
                catch (NotSupportedException x)
                {
                    throw new ArgumentException("Unable to get the position within the payload stream.", "payload", x);
                }
            }
            else
            {
                throw new ArgumentException("Length must be non-negative!", "len");
            }

            buff = new byte[buflen];

            payload.Read(buff, 0, (int)buflen);

            LowSendPacket(header, buff, cbk, state);
        }
    }
}
