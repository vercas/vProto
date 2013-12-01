using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientType = vProto.BaseClient;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.BaseClient.RequestReceived event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class RequestReceivedEventArgs
        : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.RequestReceivedEventArgs with the specified payload.
        /// </summary>
        /// <param name="payload">The request payload.</param>
        /// <param name="type">The ID of the request.</param>
        public RequestReceivedEventArgs(byte[] payload, short type)
            : base()
        {
            Payload = payload;
            if (payload != null)
                PayloadStream = new System.IO.MemoryStream(payload);

            Type = type;
        }


        /// <summary>
        /// Gets a memory stream over the payload.
        /// </summary>
        public System.IO.MemoryStream PayloadStream { get; private set; }
        /// <summary>
        /// Gets the array of bytes that make up the request payload.
        /// </summary>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Gets the ID associated with this request.
        /// </summary>
        public Int16 Type { get; private set; }
    }
}
