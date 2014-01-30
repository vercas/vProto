using System;
using System.Collections.Generic;
using System.Threading;

namespace vProto
{
    using Internals;
    using Packages;

    partial class BaseClient
    {
#if SENDER_THREAD
        bool LowSendPacket(PackageHeader header, byte[] bodeh, Action cbk = null, object state = null)
        {
            if (Disposed)
                throw new ObjectDisposedException("vProto.BaseClient", "This client base is disposed of!");

            if (!sender.IsAlive)
                sender.Start();
            
            header.Size = (uint)bodeh.Length;

            var st = new Package(header, bodeh) { State = state };

            if (cbk != null)
                st.Callbacks.Add(cbk);

            lock (send_lock)
            {
                sendingBuffer.Write(Struct_mapping.StructureToByteArray(header), 0, packetHeaderSize);
                sendingBuffer.Write(bodeh, 0, bodeh.Length);

                sendingQueue.Enqueue(st);
            }

            return true;
        }



        private object send_lock = new object();

        Queue<Package> sendingQueue = new Queue<Package>();
        Queue<Package> backQueue = new Queue<Package>();

        System.IO.MemoryStream sendingBuffer = new System.IO.MemoryStream();
        System.IO.MemoryStream backBuffer = new System.IO.MemoryStream();

        //  Swapped buffers. Brilliant idea.

        protected void SenderLoop()
        {
            Queue<Package> tempQueue;
            System.IO.MemoryStream tempBuffer;

            int tempWriteNum;
            byte[] tempWriteBuffer = new byte[64 * 1024];   //  To be honest, there IS enough RAM.

            while (!Disposed)
            {
                //tempB.Seek(0, System.IO.SeekOrigin.Begin);
                //tempB.SetLength(0);

                //  Emptying the old buffa' and the queue.
                backQueue.Clear();

                backBuffer.Seek(0, System.IO.SeekOrigin.Begin);
                backBuffer.SetLength(0);

                lock (send_lock)
                {
                    //  Buffer shift!

                    tempQueue = sendingQueue;
                    sendingQueue = backQueue;
                    backQueue = tempQueue;

                    tempBuffer = sendingBuffer;
                    sendingBuffer = backBuffer;
                    backBuffer = tempBuffer;
                }

                if (backQueue.Count > 0)
                {
                    backBuffer.Seek(0, System.IO.SeekOrigin.Begin);

                    //Console.WriteLine("Sending {0} packages, totalling in {1} bytes!", backQueue.Count, backBuffer.Length);

                    try
                    {
                        while ((tempWriteNum = backBuffer.Read(tempWriteBuffer, 0, tempWriteBuffer.Length)) != 0)
                        {
                            streamIn.Write(tempWriteBuffer, 0, tempWriteNum);

                            __addSent(tempWriteNum);
                        }
                    }
                    catch (ObjectDisposedException x)
                    {
                        _CheckIfStopped(x);

                        return;
                    }
                    catch (Exception x)
                    {
                        foreach (var pack in backQueue)
                            _OnPipeFailure(x, true, pack);

                        return;
                    }

                    foreach (var pack in backQueue)
                    {
                        foreach (var cbk in pack.Callbacks)
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

#else

        /// <summary>
        /// Gets a value indicating whether a packet is being sent.
        /// <para>Attempting to send anything else while this value is true will add the package to a queue, which will be sent as soon as the pipe is free.</para>
        /// </summary>
        public Boolean IsSendingPacket { get { return sending; } }
        //  I don't know if making a property whose existence depends on a preprocessor directive is a good idea...


        private object send_lock = new object();
        private bool sending = false;
        private DateTime sendStartTime;

        Queue<QueuedPackage> packageQueue = new Queue<QueuedPackage>();

        bool LowSendPacket(PackageHeader header, byte[] bodeh, Action cbk = null, object state = null)
        {
            if (!IsConnected)
                return false;

            header.Size = (uint)bodeh.Length;

            byte[] pack = new byte[bodeh.Length + packetHeaderSize];

            bodeh.CopyTo(pack,
                Struct_mapping.StructureToByteArray(header, pack, 0) );

            //  The StructureToByteArray function will write the structure to the specified byte array starting at the specified index, and return the length.
            //  Thus, the copying of the body starts 1 unit after the end of the header.

            var st = new Package(header, bodeh) { State = state };

            if (cbk != null)
                st.Callbacks.Add(cbk);

            return __sendPack(pack, LowSendPacket_Callback, st, false);
        }

        void LowSendPacket_Callback(IAsyncResult ar)
        {
            //Console.Write("Package sending callback; ");

            try
            {
                lock (send_lock)
                {
                    streamIn.EndWrite(ar);

                    sending = packageQueue.Count > 0;

                    //Console.Write("Sending: {0}; Queue count: {1}; ", sending, packageQueue.Count);
                }
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);

                return;
            }
            catch (Exception x)
            {
                _OnPipeFailure(x, true, ar.AsyncState as Package);

                return;
            }

            var state = ar.AsyncState as Package;
            state.time = DateTime.Now - sendStartTime;
            var bytes = state.Payload.Length + packetHeaderSize;

            __addSent(state.time.TotalSeconds > 1.0 ? (int)((double)bytes / state.time.TotalSeconds) : bytes);
            //  This has to be calculated depending on the timespan for the sake of delivering accurate information.
            //  Otherwise, sending a 100-megabyte packet will display an outgoing speed of 100 megabytes for a second when the package was successfully sent.
            //  The check is there because anything that took under one second will be handled by the timer. Also, preventing a division by 0.

            try
            {
                if (state != null)
                {
                    foreach (var cbk in state.Callbacks)
                        cbk();

                    OnInternalPacketSent(state);
                }
                //else dafuq?
            }
            finally
            {
                QueuedPackage t = null;

                lock (send_lock)
                    if (packageQueue.Count > 0)
                        t = packageQueue.Dequeue();

                //Console.WriteLine((t == null) ? "Nothing queued." : "Item queued!");

                if (t != null)
                    __sendPack(t, true);
            }
        }


        bool __sendPack(QueuedPackage pck, bool force) { return __sendPack(pck.Data, pck.AsynchronousCallback, pck.PackageObject, force); }

        bool __sendPack(byte[] pack, AsyncCallback cbk, Package st, bool force)
        {
            //Console.Write("Sending package: {0}; Forced: {1}; ", st.Header.Type, force);

            lock (send_lock)
            {
                //Console.Write("Sending: {0}; ", sending);

                //  If we're sending and we're not being forced to send...
                if (sending && !force)
                {
                    packageQueue.Enqueue(new QueuedPackage(pack, cbk, st));

                    //Console.WriteLine("Queued.");
                }
                else
                {
                    sending = true;

                    try
                    {
                        sendStartTime = DateTime.Now;

                        streamIn.BeginWrite(pack, 0, pack.Length, cbk, st);

                        //Console.WriteLine("Sent!");
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
                }
            }

            return true;
        }//*/
#endif


        //  These functions also check their arguments.

        internal protected void _SendPack(byte[] payload, PackageHeader header, Action cbk = null, object state = null)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (payload == null)
                throw new ArgumentNullException("payload", "Payload array cannot be null.");

            LowSendPacket(header, payload, cbk, state);
        }

        internal protected void _SendPack(System.IO.Stream payload, PackageHeader header, int? len = null, Action cbk = null, object state = null)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

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
