using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.BaseClient.AuthFailed event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class ClientAuthFailedEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ClientAuthFailedEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public ClientAuthFailedEventArgs(Exception x)
            : base(x)
        {
            
        }
    }
}
