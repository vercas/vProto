using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace vProto
{
    using Collections;
    using Internals;

    /// <summary>
    /// Base class for objects which handle network streams.
    /// </summary>
    public abstract partial class BaseClient
        : _RequestHandler, IDisposable
    {
        /// <summary>
        /// The internal vProto protocol version of the available library.
        /// </summary>
        public static readonly Version ProtocolVersion = new Version(0, 1);

        private static readonly byte[] emptyPayload = new byte[0];
        internal static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);



#if RECEIVER_THREAD
        /// <summary>
        /// Thread used to receive packages.
        /// </summary>
        protected Thread receiver;
#endif

#if SENDER_THREAD
        /// <summary>
        /// Thread used to send packages.
        /// </summary>
        protected Thread sender;
#endif

        /// <summary>
        /// Input stream (to the other side).
        /// <para>Used to send packages.</para>
        /// </summary>
        protected Stream streamSender;
        /// <summary>
        /// Output stream (from the other side).
        /// <para>Used to receive packages.</para>
        /// </summary>
        protected Stream streamReceiver;



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
                return;

            Disposed = true;
            IsInternallyConnected = false;
            IsConnected = false;

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

            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Gets a value indicating whether the client is known to be connected or not.
        /// <para>The handshake must complete for a connection to be declared.</para>
        /// </summary>
        public Boolean IsConnected { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the client is known to be connected or not.
        /// </summary>
        internal protected Boolean IsInternallyConnected { get; protected set; }

        /// <summary>
        /// Halts the connection and raises the appropriate events.
        /// </summary>
        /// <param name="x">optional; Exception which caused the halting.</param>
        /// <param name="force">True to force the procedure even if the client is technically not connected yet.</param>
        protected void _CheckIfStopped(Exception x, bool force = false)
        {
            if (IsInternallyConnected || force)
            {
                var isc = IsConnected;

                Dispose();

                if (isc)
                    OnDisconnected(new Events.ClientDisconnectedEventArgs(x));
                else
                    OnConnectionFailed(new Events.ClientConnectionFailedEventArgs(x));
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
        /// <param name="strReceiver">The communication input stream. This is the stream which is checked for incomming packages (read from).</param>
        /// <param name="strSender">optional; The communication output stream. This is the stream which is given outgoing packages (written to). If null, will be the same as the input stream.</param>
        protected void InitializeFromStreams(Stream strReceiver, Stream strSender = null)
        {
            if (strReceiver == null)
                throw new ArgumentNullException("strReceiver", "Input stream may not be null!");

            Ping = TimeSpan.Zero;
            //  Default value for ping, aiding response timing.

#if RECEIVER_THREAD
            receiver = new Thread(new ThreadStart(ReceiverLoop));
#endif

#if SENDER_THREAD
            sender = new Thread(new ThreadStart(SenderLoop));
#endif

            heartbeatTimer = new Timer(__heartbeatTimerCallback, null, HeartbeatInterval, HeartbeatInterval);
            speedCountingTimer = new Timer(__speedCountingTimerCallback, null, new TimeSpan(0), new TimeSpan(0, 0, 1));

            IsInternallyConnected = true;

            streamReceiver = strReceiver;
            streamSender = strSender ?? strReceiver;

            __registerDefaultInternalRequestHandlers();

            LowStartGettingPackages();

            if (Owner != null)
            {
                //  I temporarily forgot what I was supposed to do here.

                SERVER = true;
                CLIENT = false;

                _peers = Owner.GetClientPeersID(this._id);

                _StartHandshake();
            }
        }



        /// <summary>
        /// Gets whether this vProto.BaseClient handles a server's connection to a client (true) or handles a client's connection to a server (false).
        /// <para>Equivalent to checking whether the owner is non-null.</para>
        /// </summary>
        public Boolean IsClientHandler { get { return Owner != null; } }

        /// <summary>
        /// Gets or sets the server which owns this client object.
        /// <para>If non-null, this instance becomes known as a client handler.</para>
        /// <para>Must be set before initializing the client over streams.</para>
        /// </summary>
        protected BaseServer Owner { get; set; }

        /// <summary>
        /// Used internally to determine which side the BaseClient object belongs to.
        /// </summary>
        protected bool SERVER = false, CLIENT = true;



        /// <summary>
        /// Gets or sets the object that contains data about the client.
        /// </summary>
        public Object Tag { get; set; }
    }
}
