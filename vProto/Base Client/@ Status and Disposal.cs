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

        protected Thread receiver;
        protected Thread sender;

        protected System.IO.Stream stream;


        /// <summary>
        /// Gets a value indicating whether the object is disposed or not.
        /// </summary>
        public Boolean Disposed { get; private set; }

        public void Dispose()
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

            try
            {
                sender.Abort();
            }
            catch (Exception)
            {

            }

            ExtraDispose();

            Disposed = true;
            IsConnected = false;
        }

        protected virtual void ExtraDispose() { }


        /// <summary>
        /// Gets a value indicating whether the client is known to be connected or not.
        /// </summary>
        public Boolean IsConnected { get; protected set; }

        protected void _CheckIfStopped(Exception x)
        {
            Console.WriteLine("CHECKING IF STOPPED OMG");

            if (IsConnected)
            {
                IsConnected = false;

                /*lock (rec_sync)
                {
                    receiving = false;
                }*/

                /*try
                {
                    receiver.Abort();
                }
                catch (Exception)
                {

                }

                try
                {
                    sender.Abort();
                }
                catch (Exception)
                {

                }

                try
                {
                    client.Close();
                }
                finally
                {
                    OnDisconnected(new Events.ClientDisconnectedEventArgs(x));
                }*/

                Dispose();

                OnDisconnected(new Events.ClientDisconnectedEventArgs(x));
            }
        }
    }
}
