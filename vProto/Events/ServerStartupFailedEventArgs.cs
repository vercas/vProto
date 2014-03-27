using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Server.ServerStartupFailed event. This class cannot be inherited.
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
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
