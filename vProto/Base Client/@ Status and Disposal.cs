using System;
using System.IO;
using System.Threading;

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
#if RECEIVER_THREAD
        protected Thread receiver;
#endif

#if SENDER_THREAD
        protected Thread sender;
#endif

        protected Stream streamIn;
        protected Stream streamOut;



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
            //Console.WriteLine("CHECKING IF STOPPED OMG");

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
        protected void InitializeFromStreams(Stream strIn, Stream strOut = null)
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

            heartbeatTimer = new Timer(__heartbeatTimerCallback, null, HeartbeatInterval, HeartbeatInterval);
            speedCountingTimer = new Timer(__speedCountingTimerCallback, null, new TimeSpan(0), new TimeSpan(0, 0, 1));

            IsConnected = true;

            streamIn = strIn;
            streamOut = strOut ?? strIn;

            if (Owner != null)
            {
                //  I temporarily forgot what I was supposed to do here.
            }

            __registerDefaultInternalRequestHandlers();

            OnConnected(new EventArgs());

            LowStartGettingPackets();
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
    }
}
