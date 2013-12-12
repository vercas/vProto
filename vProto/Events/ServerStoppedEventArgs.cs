using System;

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
        public ServerStoppedEventArgs(Exception x)
            : base(x)
        {
            
        }
    }
}
