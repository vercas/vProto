using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExceptionType = System.IO.IOException;

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
        public ListeningFailedEventArgs(ExceptionType x)
            : base(x)
        {
            
        }
    }
}
