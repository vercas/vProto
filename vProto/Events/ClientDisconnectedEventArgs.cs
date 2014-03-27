using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.BaseClient.Disconnected event. This class cannot be inherited.
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    public sealed class ClientDisconnectedEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ClientDisconnectedEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public ClientDisconnectedEventArgs(Exception x)
            : base(x)
        {
            
        }
    }
}
