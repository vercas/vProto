﻿using System;

namespace vProto.Events
{
    /// <summary>
    /// Base class for events which may carry an exception.
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    public class ExceptionCarryingEventArgs
        : EventArgs
    {
        /// <summary>
        /// Event argument which may carry an exception.
        /// </summary>
        /// <param name="x">The possible exception.</param>
        public ExceptionCarryingEventArgs(Exception x)
            : base()
        {
            Exception = x;
        }


        /// <summary>
        /// Gets the exception carried by this event.
        /// </summary>
        public Exception Exception { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this event carries an exception or not.
        /// </summary>
        public Boolean CarriesException { get { return Exception != null; } }
    }
}
