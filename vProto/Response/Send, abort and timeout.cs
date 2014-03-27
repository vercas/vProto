using System;

namespace vProto
{
    using Packages;

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
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot send a disposed response!");

            lock (__syncObject)
            {
                try
                {
                    client._SendPack(str, new PackageHeader() { IDTop = id, IDBottom = Type, Type = isInternal ? PackageType.InternalResponse : PackageType.Response }, null, null, this);
                }
                catch { }

                try
                {
                    str.Dispose();

                    reqstr.Dispose();
                }
                catch { }
                finally
                {
                    Disposed = Sent = true;
                }
            }

            return this;
        }

        /// <summary>
        /// Marks the response as aborted and disposed, leaving the request to time out on the sender's side.
        /// </summary>
        /// <returns></returns>
        public Response Abort()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot abort a disposed response!");

            lock (__syncObject)
            {
                try
                {
                    if (str != null)
                        str.Dispose();

                    reqstr.Dispose();
                }
                catch { }
                finally
                {
                    Disposed = Aborted = true;

                    //OnresponseAborted(new EventArgs());
                }
            }

            return this;
        }

        internal void DeclareTimeout()
        {
            lock (__syncObject)
            {
                try
                {
                    if (str != null)
                        str.Dispose();

                    reqstr.Dispose();
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
}
