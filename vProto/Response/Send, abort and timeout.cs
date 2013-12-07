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
    partial class Response
    {
        /// <summary>
        /// Attempts to send the response.
        /// <para>Upon success, the response is marked as sent and disposed of.</para>
        /// </summary>
        /// <returns></returns>
        public Response Send()
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot send a disposed response!", (Exception)null);

            client._SendPack(str, new Packages.PackageHeader() { IDTop = id, IDBottom = Type, Type = Packages.PackageType.Response }, null, null, this);

            Sent = Disposed = true;

            try
            {
                str.Close();

                reqstr.Close();
            }
            catch { }

            return this;
        }

        /// <summary>
        /// Marks the response as aborted and disposed, leaving the request to time out on the sender's side.
        /// </summary>
        /// <returns></returns>
        public Response Abort()
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot abort a disposed response!", (Exception)null);

            try
            {
                if (str != null)
                    str.Close();

                reqstr.Close();
            }
            catch { }
            finally
            {
                Disposed = true;
                Aborted = true;

                //OnresponseAborted(new EventArgs());
            }

            return this;
        }

        internal void DeclareTimeout()
        {
            try
            {
                if (str != null)
                    str.Close();

                reqstr.Close();
            }
            catch { }
            finally
            {
                Disposed = TimedOut = true;

                //OnRequestTimeout(new EventArgs());
            }
        }
    }
}
