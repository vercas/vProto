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
    public class Request
        : IDisposable
    {
        /// <summary>
        /// Default request timeout.
        /// <para>10 seconds.</para>
        /// </summary>
        public const int DefaultTimeout = 10000;
        /// <summary>
        /// Maximum timeout value.
        /// <para>655360 milliseconds or 655 seconds or 10 minutes and 55 seconds.</para>
        /// </summary>
        public const int MaxTimeout = ((int)ushort.MaxValue) * 10;
        /// <summary>
        /// Minimum timeout value.
        /// <para>It's 1 millisecond.</para>
        /// </summary>
        public const int MinTimeout = 1;


        #region IDisposable Implementation, Status and Finalizer
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
        /// Gets a value indicating whether the request is (still) pending.
        /// <para>A pending request hasn't been sent or aborted (yet).</para>
        /// </summary>
        public Boolean Pending { get { return !(Sent || Aborted || Disposed); } }

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
        #endregion

        internal BaseClient client;
        short id;

        internal bool responded = false;

        System.IO.MemoryStream str = null;


        #region Properties
        #region State
        /// <summary>
        /// Gets or sets an object which represents the state of the request.
        /// </summary>
        public Object State { get; set; }
        /// <summary>
        /// Sets the state object of the request.
        /// </summary>
        /// <param name="value">The state object.</param>
        /// <returns>The request object.</returns>
        public Request SetStateObject(Object value)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            State = value;
            return this;
        }
        #endregion


        #region Type
        /// <summary>
        /// Gets or sets a short representing the application-level request type.
        /// </summary>
        public Int16 Type { get; set; }
        /// <summary>
        /// Sets the type of the request.
        /// </summary>
        /// <param name="value">The type.</param>
        /// <returns>The request object.</returns>
        public Request SetType(Int16 value)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            Type = value;
            return this;
        }
        #endregion


        #region Timeout
        private int _timeout = DefaultTimeout;
        private ushort __timeout = (ushort)(DefaultTimeout / 10);
        /// <summary>
        /// Gets or sets the number of milliseconds to wait for a response to this request before declaring timeout.
        /// </summary>
        public Int32 Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                if (value < MinTimeout)
                    throw new ArgumentOutOfRangeException("value", "Value must be strictly positive!");

                if (value > MaxTimeout)
                    throw new ArgumentOutOfRangeException("value", "Value cannot exceed " + MaxTimeout + " milliseconds!");

                _timeout = value;
                __timeout = (ushort)(value / 10);
                //  Best cheat in the world.
            }
        }
        /// <summary>
        /// Sets the timeout of the request.
        /// </summary>
        /// <param name="value">The timeout amount in milliseconds.</param>
        /// <returns>The request object.</returns>
        public Request SetTimeout(Int32 value)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            Timeout = value;
            return this;
        }
        #endregion
        #endregion


        #region Events

        #endregion


        #region Payload
        /// <summary>
        /// Sets the payload of the request.
        /// </summary>
        /// <param name="bt">Array of bytes constituting the payload.</param>
        /// <returns>The request object.</returns>
        public Request SetPayload(byte[] bt)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            if (bt == null)
                throw new ArgumentNullException("bt", "Payload cannot be null!");

            if (str != null)
                try
                {
                    str.Dispose();
                }
                catch { }

            str = new System.IO.MemoryStream(bt);
            //str.Write(bt, 0, bt.Length);

            return this;
        }

        /// <summary>
        /// Extracts the contents of the given stream according to the parameters and assigns them as the request payload.
        /// </summary>
        /// <param name="stream">The stream from which the data is extracted.</param>
        /// <param name="offset">The offset at which to begin extraction relative to the seek origin.</param>
        /// <param name="length">The number of bytes to copy. Usage of a negative number means copying everything to the end of the stream.</param>
        /// <param name="origin">The point from which seeking in the stream should take place.</param>
        /// <returns>The request object.</returns>
        public Request SetPayload(System.IO.Stream stream, int length = -1, int offset = 0, System.IO.SeekOrigin origin = System.IO.SeekOrigin.Begin)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            if (stream == null)
                throw new ArgumentNullException("bt", "Payload cannot be null!");

            if (str != null)
                try
                {
                    str.Dispose();
                }
                catch { }

            stream.Seek(offset, origin);

            if (length < 0)
                length = (int)(stream.Length - stream.Position);

            var ba = new byte[length];

            int read = 0;

            while (read < length)
            {
                read += stream.Read(ba, read, length - read);
            }

            str = new System.IO.MemoryStream(ba);

            return this;
        }

        /// <summary>
        /// Extracts the contents of the given stream according to the parameters and assigns them as the request payload.
        /// </summary>
        /// <param name="stream">The stream from which the data is extracted.</param>
        /// <param name="length">The number of bytes to copy. Usage of a negative number means copying everything to the end of the stream.</param>
        /// <returns>The request object.</returns>
        public Request SetPayload(System.IO.Stream stream, int length = -1)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            if (stream == null)
                throw new ArgumentNullException("bt", "Payload cannot be null!");

            if (str != null)
                try
                {
                    str.Dispose();
                }
                catch { }

            if (length < 0)
                length = (int)(stream.Length - stream.Position);

            var ba = new byte[length];

            int read = 0;

            while (read < length)
            {
                read += stream.Read(ba, read, length - read);
            }

            str = new System.IO.MemoryStream(ba);

            return this;
        }
        #endregion


        //  Only internally...
        internal Request(BaseClient cl, short id)
        {
            client = cl;
            this.id = id;

            Disposed = Sent = Aborted = false;
        }


        /// <summary>
        /// Attempts to send the request.
        /// <para>Upon success, the request is marked as sent and disposed of.</para>
        /// </summary>
        /// <returns></returns>
        public Request Send()
        {
            if (Disposed)
                throw new ObjectDisposedException(Sent ? "Cannot resend a request."
                    : (Aborted ? "Cannot send an aborted request!" :
                    "Cannot send a disposed request!"), (Exception)null);

            client._SendPack(str, new Packages.PackageHeader() { IDTop = id, IDBottom = Type, Type = Packages.PackageType.Request, RequestTimeout = __timeout }, null, null, this);

            Sent = Disposed = true;

            try
            {
                str.Close();
            }
            catch { }

            return this;
        }

        public Request Abort()
        {
            if (Disposed)
                throw new ObjectDisposedException(Sent ? "Cannot abort a sent request!"
                    : (Aborted ? "Request already aborted." :
                    "Cannot abort a disposed request!"), (Exception)null);

            try
            {
                if (str != null)
                    str.Close();
            }
            catch { }
            finally
            {
                Disposed = true;
                Aborted = true;
            }

            return this;
        }
    }
}
