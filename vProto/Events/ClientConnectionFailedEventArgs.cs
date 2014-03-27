using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Server.ClientConnectionFailed event. This class cannot be inherited.
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    public sealed class ClientConnectionFailedEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ClientConnectionFailedEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public ClientConnectionFailedEventArgs(Exception x)
            : base(x)
        {
            
        }
    }
}
