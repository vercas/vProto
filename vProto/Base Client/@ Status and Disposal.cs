﻿using System;
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
    /// <summary>
    /// Base class for objects which handle network streams.
    /// </summary>
    public abstract partial class BaseClient
        : IDisposable
    {
#if RECEIVER_THREAD
        protected Thread receiver;
#endif

#if SENDER_THREAD
        protected Thread sender;
#endif

        protected System.IO.Stream streamIn;
        protected System.IO.Stream streamOut;



        /// <summary>
        /// Gets a value indicating whether the object is disposed or not.
        /// </summary>
        public Boolean Disposed { get; private set; }

        /// <summary>
        /// Releases all the resources used by the current instance of vProto.BaseClient.
        /// </summary>
        public virtual void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException("vProto.BaseClient", "Object already disposed!");

            try
            {
                heartbeatTimer.Dispose();
            }
            catch (Exception)
            {

            }

            try
            {
                heartbeatTimeoutTimer.Dispose();
            }
            catch (Exception)
            {

            }

            try
            {
                speedCountingTimer.Dispose();
            }
            catch (Exception)
            {

            }

            //  First make sure the receiver is not sleeping. Then abort it.

#if RECEIVER_THREAD
            try
            {
                receiver.Interrupt();
            }
            catch (Exception)
            {

            }

            try
            {
                receiver.Abort();
            }
            catch (Exception)
            {

            }
#endif

#if SENDER_THREAD
            try
            {
                sender.Abort();
            }
            catch (Exception)
            {

            }
#endif

            Disposed = true;
            IsConnected = false;
        }


        /// <summary>
        /// Gets a value indicating whether the client is known to be connected or not.
        /// </summary>
        public Boolean IsConnected { get; protected set; }

        protected void _CheckIfStopped(Exception x, bool force = false)
        {
            Console.WriteLine("CHECKING IF STOPPED OMG");

            if (IsConnected || force)
            {
                Dispose();

                OnDisconnected(new Events.ClientDisconnectedEventArgs(x));
            }
        }



        internal Int32 _id = -1;

        /// <summary>
        /// Gets the unique ID of the client.
        /// </summary>
        public Int32 ID
        {
            get
            {
                return _id;
            }
        }



        /// <summary>
        /// Initializes the client communication through the specified stream(s).
        /// <para>The stream(s) passed on to this function are not disposed by the base client! Disposal will have to be performed by the provider.</para>
        /// </summary>
        /// <param name="strIn">The communication input stream. This is the stream which is checked for incomming packages (read from).</param>
        /// <param name="strOut">optional; The communication output stream. This is the stream which is given outgoing packages (written to). If null, will be the same as the input stream.</param>
        protected void InitializeFromStreams(System.IO.Stream strIn, System.IO.Stream strOut = null)
        {
            if (strIn == null)
                throw new ArgumentNullException("strIn", "Input stream may not be null!");

            Ping = TimeSpan.Zero;
            //  Default value for ping, aiding response timing.

#if RECEIVER_THREAD
            receiver = new Thread(new ThreadStart(ReceiverLoop));
#endif

#if SENDER_THREAD
            sender = new Thread(new ThreadStart(SenderLoop));
#endif

            heartbeatTimer = new System.Threading.Timer(__heartbeatTimerCallback, null, HeartbeatInterval, HeartbeatInterval);
            speedCountingTimer = new Timer(__speedCountingTimerCallback, null, new TimeSpan(0), new TimeSpan(0, 0, 1));

            IsConnected = true;

            streamIn = strIn;
            streamOut = strOut ?? strIn;

            OnConnected(new EventArgs());

            LowStartGettingPackets();
        }



        /// <summary>
        /// Gets whether this vProto.BaseClient handles a server's connection to a client (true) or handles a client's connection to a server (false).
        /// </summary>
        public Boolean IsClientHandler { get; protected set; }
    }
}
