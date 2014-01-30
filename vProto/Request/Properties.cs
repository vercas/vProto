using System;

namespace vProto
{
    partial class Request
    {
        /// <summary>
        /// Default request timeout.
        /// <para>10 seconds.</para>
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = new TimeSpan(0, 0, 0, 10, 0);
        /// <summary>
        /// Maximum timeout value.
        /// <para>655360 milliseconds or 655 seconds or 10 minutes and 55 seconds.</para>
        /// </summary>
        public static readonly TimeSpan MaxTimeout = new TimeSpan(0, 0, 0, 0, ((int)ushort.MaxValue) * 10);
        /// <summary>
        /// Minimum timeout value.
        /// <para>1 millisecond.</para>
        /// </summary>
        public static readonly TimeSpan MinTimeout = new TimeSpan(0, 0, 0, 0, 1);    //  For the true perfectionists out there. Somewhere.



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
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

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
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

            Type = value;
            return this;
        }
        #endregion


        #region Timeout
        private TimeSpan _timeout = DefaultTimeout;
        private ushort __timeout = (ushort)(Convert.ToInt32(DefaultTimeout.TotalMilliseconds) / 10);
        /// <summary>
        /// Gets or sets the amount of time to wait for a response to this request before declaring timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                /*if (value == TimeSpan.Zero)
                {
                    _timeout = value;
                    __timeout = 0;

                    return;
                }*/

                //  Was I retarded when I wrote the lines commented above?
                //  No timeout can mean a memory leak because request handling requires a state machine.

                if (value < MinTimeout)
                    throw new ArgumentOutOfRangeException("value", "Value must be strictly positive!");

                if (value > MaxTimeout)
                    throw new ArgumentOutOfRangeException("value", "Value cannot exceed " + MaxTimeout + " milliseconds!");

                _timeout = value;
                __timeout = (ushort)(Convert.ToInt32(value.TotalMilliseconds) / 10);
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
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

            Timeout = new TimeSpan(0, 0, 0, 0, value);
            return this;
        }
        /// <summary>
        /// Sets the timeout of the request.
        /// </summary>
        /// <param name="value">The timeout amount.</param>
        /// <returns>The request object.</returns>
        public Request SetTimeout(TimeSpan value)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

            Timeout = value;
            return this;
        }
        #endregion
    }
}
