using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.BaseClient.SendFailed or vProto.BaseClient.ReceiptFailed events. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class PipeFailureEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.PipeFailureEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public PipeFailureEventArgs(Exception x, bool outgoing = false)
            : base(x)
        {
            Outgoing = outgoing;
        }


        /// <summary>
        /// Gets a value indicating whether the error occurred when sending data or when receiving data.
        /// </summary>
        public Boolean Outgoing { get; private set; }
    }
}
