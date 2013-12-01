using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientType = vProto.BaseClient;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Request.ResponseReceived event. This class cannot be inherited.
    /// </summary>
    [Serializable]
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
            if (payload != null)
                PayloadStream = new System.IO.MemoryStream(payload);
        }


        /// <summary>
        /// Gets a memory stream over the payload.
        /// </summary>
        public System.IO.MemoryStream PayloadStream { get; private set; }
        /// <summary>
        /// Gets the array of bytes that make up the Response payload.
        /// </summary>
        public byte[] Payload { get; private set; }
    }
}
