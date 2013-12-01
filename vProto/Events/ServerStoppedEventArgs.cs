using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExceptionType = System.Exception;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Server.Disconnected event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class ServerStoppedEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ServerStoppedEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public ServerStoppedEventArgs(ExceptionType x)
            : base(x)
        {
            
        }
    }
}
