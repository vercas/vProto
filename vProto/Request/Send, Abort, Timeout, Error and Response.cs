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
        /// Attempts to send the request.
        /// <para>Upon success, the request is marked as sent and disposed of.</para>
        /// </summary>
        /// <returns></returns>
        public Request Send()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot send a disposed request!");

            client._SendPack(str, new Packages.PackageHeader() { IDTop = id, IDBottom = Type, Type = Packages.PackageType.Request, RequestTimeout = __timeout }, null, null, this);

            Sent = Disposed = true;

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
        /// <returns></returns>
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
    }
}
