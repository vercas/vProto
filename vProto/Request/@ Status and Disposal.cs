using System;

namespace vProto
{
    /// <summary>
    /// Desc
    /// </summary>
    public sealed partial class Request
        : IDisposable
    {
        private object __syncObject = new object();



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
        /// Gets a value indicating whether the request produced an exception.
        /// </summary>
        public Boolean Failed { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the request has been responded to.
        /// </summary>
        public Boolean Responded { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the request is pending.
        /// <para>A pending request hasn't been sent or aborted yet, hasn't timed out and hasn't failed.</para>
        /// </summary>
        public Boolean Pending { get { return !(Disposed || Sent || Aborted || TimedOut || Failed || Responded); } }

        /// <summary>
        /// Gets a value indicating whether the request is sent and awaiting a result.
        /// <para>Possible results are a response, failure or timeout.</para>
        /// </summary>
        public Boolean AwaitingResult { get { return Sent && !(TimedOut || Failed || Responded); } }

        /// <summary>
        /// Sends the request and closes it, cleaning up resources and preventing changes.
        /// <para>The disposal happens even if the sending failed.</para>
        /// </summary>
        public void Dispose()
        {
            if (Disposed)
                return;

            try
            {
                SendFluent();

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


        /// <summary>
        /// Attempts to fix the mistake of a careless user.
        /// </summary>
        ~Request()
        {
            Dispose();
        }

        internal BaseClient client;
        internal short id;
        internal bool isInternal = false;


        //  Only internally...
        internal Request(BaseClient cl, short id)
        {
            client = cl;
            this.id = id;

            Disposed = Sent = Aborted = TimedOut = Failed = Responded = false;
        }
    }
}
