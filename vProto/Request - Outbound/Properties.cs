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
    partial class OutboundRequest
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
        /// <para>1 millisecond.</para>
        /// </summary>
        public const int MinTimeout = 1;    //  For the true perfectionists out there. Somewhere.



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
        public OutboundRequest SetStateObject(Object value)
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
        public OutboundRequest SetType(Int16 value)
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
        /// <para>0 means no timeout!</para>
        /// </summary>
        public Int32 Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                if (value == 0)
                {
                    _timeout = __timeout = 0;
                    return;
                }

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
        public OutboundRequest SetTimeout(Int32 value)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            Timeout = value;
            return this;
        }
        #endregion
    }
}
