﻿using System;
using System.Collections.Generic;
using System.Threading;

#if NETFX_CORE
using Windows.System.Threading;
#endif

namespace vProto
{
    using Internals;
    using Packages;

    partial class BaseClient
    {
#if SENDER_THREAD
        unsafe bool LowSendPackage(PackageHeader header, byte[] bodeh, Action cbk = null, object state = null)
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
                //sendingBuffer.Write(Struct_mapping.StructureToByteArray(header), 0, packageHeaderSize);
                sendingBuffer.Write(header.Buffer, 0, packageHeaderSize);
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

        void SenderLoop()
        {
            Queue<Package> tempQueue;
            System.IO.MemoryStream tempBuffer;

            int tempWriteNum;
            byte[] tempWriteBuffer = new byte[64 * 1024];   //  To be honest, there IS enough RAM.

            while (!Disposed)
            {
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
                            streamSender.Write(tempWriteBuffer, 0, tempWriteNum);

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

                        OnInternalPackageSent(pack);
                    }
                }
                else
                    Thread.Sleep(1);

                //  Thread.Yield seems to cause enormous CPU spikes.
            }
        }

#else

        /*/// <summary>
        /// Gets a value indicating whether a package is being sent.
        /// <para>Attempting to send anything else while this value is true will add the package to a queue, which will be sent as soon as the pipe is free.</para>
        /// </summary>
        public Boolean IsSendingPackage { get { return sending; } }
        //  I don't know if making a property whose existence depends on a preprocessor directive is a good idea...


        private object send_lock = new object();
        private bool sending = false;
        private DateTime sendStartTime;

        Queue<QueuedPackage> packageQueue = new Queue<QueuedPackage>();

        bool LowSendPackage(PackageHeader header, byte[] bodeh, Action cbk = null, object state = null)
        {
            if (!IsConnected)
                return false;

            header.Size = (uint)bodeh.Length;

            byte[] pack = new byte[bodeh.Length + packageHeaderSize];

            bodeh.CopyTo(pack,
                Struct_mapping.StructureToByteArray(header, pack, 0) );

            //  The StructureToByteArray function will write the structure to the specified byte array starting at the specified index, and return the length.
            //  Thus, the copying of the body starts 1 unit after the end of the header.

            var st = new Package(header, bodeh) { State = state };

            if (cbk != null)
                st.Callbacks.Add(cbk);

            return __sendPack(pack, LowSendPackage_Callback, st, false);
        }

        void LowSendPackage_Callback(IAsyncResult ar)
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
            var bytes = state.Payload.Length + packageHeaderSize;

            __addSent(state.time.TotalSeconds > 1.0 ? (int)((double)bytes / state.time.TotalSeconds) : bytes);
            //  This has to be calculated depending on the timespan for the sake of delivering accurate information.
            //  Otherwise, sending a 100-megabyte package will display an outgoing speed of 100 megabytes for a second when the package was successfully sent.
            //  The check is there because anything that took under one second will be handled by the timer. Also, preventing a division by 0.

            try
            {
                if (state != null)
                {
                    foreach (var cbk in state.Callbacks)
                        cbk();

                    OnInternalPackageSent(state);
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

        unsafe bool LowSendPackage(PackageHeader header, byte[] bodeh, Action cbk = null, object state = null)
        {
            if (Disposed)
                throw new ObjectDisposedException("vProto.BaseClient", "This client base is disposed of!");

            header.Size = (uint)bodeh.Length;

            var st = new Package(header, bodeh) { State = state };

            if (cbk != null)
                st.Callbacks.Add(cbk);

            lock (send_lock)
            {
                sendingBuffer.Write(header.Buffer, 0, packageHeaderSize);
                sendingBuffer.Write(bodeh, 0, bodeh.Length);

                sendingQueue.Enqueue(st);

                if (!queued)
                {
                    queued = true;

#if NETFX_CORE
#pragma warning disable 4014
                    ThreadPool.RunAsync(SenderLoop);
#pragma warning restore 4014
#else
                    ThreadPool.QueueUserWorkItem(SenderLoop);
#endif
                }
            }

            return true;
        }



        private object send_lock = new object();
        bool queued = false;

        Queue<Package> sendingQueue = new Queue<Package>();
        Queue<Package> backQueue = new Queue<Package>();

        System.IO.MemoryStream sendingBuffer = new System.IO.MemoryStream();
        System.IO.MemoryStream backBuffer = new System.IO.MemoryStream();

        //  Swapped buffers. Brilliant idea.

        void SenderLoop(object state)
        {
            Queue<Package> tempQueue;
            System.IO.MemoryStream tempBuffer;

            int tempWriteNum;
            byte[] tempWriteBuffer = new byte[64 * 1024];   //  To be honest, there IS enough RAM.

            bool go = true;

            while (go)
            {
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

                backBuffer.Seek(0, System.IO.SeekOrigin.Begin);

                try
                {
                    while ((tempWriteNum = backBuffer.Read(tempWriteBuffer, 0, tempWriteBuffer.Length)) != 0)
                    {
                        streamSender.Write(tempWriteBuffer, 0, tempWriteNum);

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

                    OnInternalPackageSent(pack);
                }

                lock(send_lock)
                    if (sendingQueue.Count == 0)
                        queued = go = false;
            }
        }
#endif


        //  These functions also check their arguments.

        /// <summary>
        /// Sends the specified byte array as payload for a package.
        /// <para>Size is automatically set in header.</para>
        /// </summary>
        /// <param name="payload">Body of package.</param>
        /// <param name="header">Header of package.</param>
        /// <param name="cbk">Action to execute on successful sending.</param>
        /// <param name="state">State object associated with the package for internal use.</param>
        internal protected void _SendPack(byte[] payload, PackageHeader header, Action cbk = null, object state = null)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (payload == null)
                throw new ArgumentNullException("payload", "Payload array cannot be null.");

            LowSendPackage(header, payload, cbk, state);
        }

        /// <summary>
        /// Sends from the specified stream as payload for a package.
        /// <para>Size is automatically set in header.</para>
        /// </summary>
        /// <param name="payload">The stream from which to pick the body.</param>
        /// <param name="header">Header of package.</param>
        /// <param name="len">How many bytes to copy from current position on stream; use null to copy until the end.</param>
        /// <param name="cbk">Action to execute on successful sending.</param>
        /// <param name="state">State object associated with the package for internal use.</param>
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

            LowSendPackage(header, buff, cbk, state);
        }
    }
}
