using System;
using System.IO;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.BaseClient.ListeningFailed event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class ListeningFailedEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ListeningFailedEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public ListeningFailedEventArgs(IOException x)
            : base(x)
        {
            
        }
    }
}
