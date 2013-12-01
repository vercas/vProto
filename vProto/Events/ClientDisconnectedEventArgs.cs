using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExceptionType = System.Exception;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.BaseClient.Disconnected event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class ClientDisconnectedEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ClientDisconnectedEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public ClientDisconnectedEventArgs(ExceptionType x)
            : base(x)
        {
            
        }
    }
}
