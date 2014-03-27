using System;
using System.IO;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Request.ResponseReceived event. This class cannot be inherited.
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    public sealed class ResponseReceivedEventArgs
        : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ResponseReceivedEventArgs with the specified payload and package ID.
        /// </summary>
        /// <param name="payload">The response payload.</param>
        public ResponseReceivedEventArgs(byte[] payload)
            : base()
        {
            Payload = payload;
            //if (payload != null)
            //    PayloadStream = new MemoryStream(payload);
        }


        /*/// <summary>
        /// Gets a memory stream over the payload.
        /// </summary>
        public MemoryStream PayloadStream { get; private set; }*/
        /// <summary>
        /// Gets the array of bytes that make up the Response payload.
        /// </summary>
        public byte[] Payload { get; private set; }
    }
}
