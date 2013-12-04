using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Server.ServerStartupFailed event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class ServerStartupFailedEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ServerStartupFailedEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public ServerStartupFailedEventArgs(Exception x)
            : base(x)
        {
            
        }
    }
}
