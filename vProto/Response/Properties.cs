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
        /// Gets or sets an object which represents the state of the response.
        /// </summary>
        public Object State { get; set; }

        /// <summary>
        /// Sets the state object of the response.
        /// </summary>
        /// <param name="value">The state object.</param>
        /// <returns>The response object.</returns>
        public Response SetStateObject(Object value)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed response!", (Exception)null);

            State = value;
            return this;
        }



        /// <summary>
        /// Gets or sets a short representing the application-level response type.
        /// </summary>
        public Int16 Type { get; private set; }



        /// <summary>
        /// Gets the amount of time left to send the response.
        /// </summary>
        public TimeSpan TimeLeft { get { return TimeDue - DateTime.Now; } }

        /// <summary>
        /// Gets the amount of time elapsed since the response was asked for.
        /// </summary>
        public TimeSpan TimeElapsed { get { return DateTime.Now - TimeRquested; } }

        /// <summary>
        /// Gets the time at which the request was received.
        /// </summary>
        public DateTime TimeRquested { get; private set; }

        /// <summary>
        /// Gets the time at which the request and reply will be timed out.
        /// </summary>
        public DateTime TimeDue { get; private set; }



        /// <summary>
        /// Gets the amount of time available to send the response.
        /// </summary>
        public TimeSpan Timeout { get; private set; }
    }
}
