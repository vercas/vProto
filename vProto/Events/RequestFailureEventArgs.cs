using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Request.RequestFailure event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class RequestFailureEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.RequestFailureEventArgs with the specified exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public RequestFailureEventArgs(Exception x, bool sending = true)
            : base(x)
        {
            Sending = sending;
        }


        /// <summary>
        /// Gets a value indicating whether the error occurred when sending the request.
        /// </summary>
        public Boolean Sending { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the error occurred when receiving the response.
        /// </summary>
        public Boolean Receiving { get { return !Sending; } }

        //  This other value, which is the opposite of the first, exists to eliminate ambiguity.
    }
}
