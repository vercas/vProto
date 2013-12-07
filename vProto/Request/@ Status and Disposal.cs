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
    /// <summary>
    /// Desc
    /// </summary>
    public sealed partial class Request
        : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the request is disposed or not.
        /// </summary>
        public Boolean Disposed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request has been successfully sent.
        /// </summary>
        public Boolean Sent { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the request has been aborted.
        /// </summary>
        public Boolean Aborted { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the request has timed out.
        /// </summary>
        public Boolean TimedOut { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the request has been responded to.
        /// </summary>
        public Boolean Responded { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request is (still) pending.
        /// <para>A pending request hasn't been sent or aborted (yet).</para>
        /// </summary>
        public Boolean Pending { get { return !(Disposed || Sent || Aborted || TimedOut || Responded); } }

        /// <summary>
        /// Sends the request and closes it, cleaning up resources and preventing changes.
        /// <para>The disposal happens even if the sending failed.</para>
        /// </summary>
        public void Dispose()
        {
            if (Disposed)
                return;
            else
                try
                {
                    Send();

                    return;
                    //  We don't need the code below again, do we? :P
                }
                catch
                {
                    Sent = false;
                }

            try
            {
                str.Close();
            }
            catch { }
            finally
            {
                Disposed = true;
            }
        }


        ~Request()
        {
            try
            {
                Abort();
            }
            catch { }   //  For God's sake. :S
        }

        internal BaseClient client;
        short id;


        //  Only internally...
        internal Request(BaseClient cl, short id)
        {
            client = cl;
            this.id = id;

            Disposed = Sent = Aborted = TimedOut = Responded = false;
        }
    }
}
