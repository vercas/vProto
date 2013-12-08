using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*  A few notes:
 *  1.  try-finally doesn't catch exceptions!!
 */

namespace vProto
{
    partial class Request
    {
        /// <summary>
        /// Attempts to send the request. This process is asynchronous; unlike SendAsync, this method is not awaitable and will not deliver any result to the caller, only in events.
        /// <para>Upon success, the request is marked as sent.</para>
        /// <para>Whether this fails or not, the request will be disposed of.</para>
        /// </summary>
        /// <returns>The current request object.</returns>
        public Request SendFluent()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot send a disposed request!");

            try
            {
                client._SendPack(str, new Packages.PackageHeader() { IDTop = id, IDBottom = Type, Type = Packages.PackageType.Request, RequestTimeout = __timeout }, null, null, this);

                Sent = Disposed = true;
            }
            catch { }

            try
            {
                str.Close();
            }
            catch { }

            return this;
        }

        /// <summary>
        /// Marks the request as aborted and disposed and raises the appropriate event.
        /// </summary>
        /// <returns>The current request object.</returns>
        public Request Abort()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot abort a disposed request!");

            try
            {
                if (str != null)
                    str.Close();
            }
            catch { }
            finally
            {
                Disposed = Aborted = true;

                OnRequestAborted(new EventArgs());
            }

            return this;
        }

        internal void DeclareTimeout()
        {
            try
            {
                if (str != null)
                    str.Close();
            }
            catch { }
            finally
            {
                Disposed = TimedOut = true;

                OnRequestTimeout(new EventArgs());
            }
        }

        internal void DeclareFailure(Exception x, bool sending)
        {
            try
            {
                if (str != null)
                    str.Close();
            }
            catch { }
            finally
            {
                Disposed = Failed = true;

                OnRequestFailure(new Events.RequestFailureEventArgs(x, sending));
            }
        }

        internal void DeclareResponded(byte[] payload)
        {
            try
            {
                if (str != null)
                    str.Close();
            }
            catch { }
            finally
            {
                Disposed = Responded = true;

                OnResponseReceived(new Events.ResponseReceivedEventArgs(payload));
            }
        }



        /// <summary>
        /// Attempts to send the request. This process is asynchronous; unlike SendFluent, this method is awaitable and results will be delivered both to the caller and through events.
        /// <para>Upon success, the request is marked as sent.</para>
        /// <para>Whether this fails or not, the request will be disposed of.</para>
        /// </summary>
        /// <returns>The current request object.</returns>
        public /*async*/ Task<byte[]> SendAsync()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot send a disposed request!");

            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();

            AddResponseReceivedHandler(delegate(Request sender, BaseClient client, Events.ResponseReceivedEventArgs e)
            {
                tcs.TrySetResult(e.Payload);
            }).AddTimeoutHandler(delegate(Request sender, BaseClient client, EventArgs e)
            {
                tcs.TrySetCanceled();
            }).AddFailureHandler(delegate(Request sender, BaseClient client, Events.RequestFailureEventArgs e)
            {
                tcs.TrySetException(e.Exception);
            }).SendFluent();

            return /*await*/ tcs.Task;

            //return Task<byte[]>.Factory.FromAsync()
        }
    }
}
