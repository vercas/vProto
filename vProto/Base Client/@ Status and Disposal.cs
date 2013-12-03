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
    /// <summary>
    /// Base class for objects which handle network streams.
    /// </summary>
    public abstract partial class BaseClient
        : IDisposable
    {
        protected TcpClient client;
        protected NetworkStream Nstream;
        protected SslStream Sstream;

#if RECEIVER_THREAD
        protected Thread receiver;
#endif

#if SENDER_THREAD
        protected Thread sender;
#endif

        protected System.IO.Stream stream;



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
                client.Close();
            }
            catch (Exception)
            {

            }

            try
            {
                Sstream.Close();
            }
            catch (Exception)
            {

            }

            try
            {
                Nstream.Close();
            }
            catch (Exception)
            {

            }

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

        protected void _CheckIfStopped(Exception x)
        {
            Console.WriteLine("CHECKING IF STOPPED OMG");

            if (IsConnected)
            {
                Dispose();

                OnDisconnected(new Events.ClientDisconnectedEventArgs(x));
            }
        }
    }
}
