using System;
using System.IO;

namespace vProto
{
    /// <summary>
    /// Desc
    /// </summary>
    public sealed partial class Response
        : IDisposable
    {
        private object __syncObject = new object();



        /// <summary>
        /// Gets a value indicating whether the response is disposed or not.
        /// </summary>
        public Boolean Disposed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the response has been successfully sent.
        /// </summary>
        public Boolean Sent { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the response has been aborted.
        /// </summary>
        public Boolean Aborted { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the response has timed out.
        /// </summary>
        public Boolean TimedOut { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the response produced an exception.
        /// </summary>
        public Boolean Failed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the response is (still) pending.
        /// <para>A pending response hasn't been sent or aborted (yet).</para>
        /// </summary>
        public Boolean Pending { get { return !(Disposed || Sent || Aborted || TimedOut || Failed); } }

        /// <summary>
        /// Sends the response and closes it, cleaning up resources and preventing changes.
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
                if (str != null)
                    str.Close();

                reqstr.Close();
            }
            catch { }
            finally
            {
                Disposed = true;
            }
        }


        ~Response()
        {
            try
            {
                Abort();
            }
            catch { }   //  For God's sake. :S
        }

        internal BaseClient client;
        internal short id;
        internal bool isInternal = false;


        //  Only internally...
        internal Response(BaseClient cl, short id, short type, byte[] reqpayload, TimeSpan timeout, DateTime receivalTime)
        {
            client = cl;
            this.id = id;
            this.Type = type;

            Timeout = timeout;
            TimeRquested = receivalTime;
            TimeDue = receivalTime + timeout;

            reqarr = reqpayload;
            reqstr = new MemoryStream(reqpayload);

            Disposed = Sent = Aborted = TimedOut = Failed = false;
        }
    }
}
