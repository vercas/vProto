using System;
using System.IO;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.BaseClient.DataReceived event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class DataReceivedEventArgs
        : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.DataReceivedEventArgs with the specified payload and package ID.
        /// </summary>
        /// <param name="payload">The data payload.</param>
        /// <param name="type">The ID of the data package.</param>
        public DataReceivedEventArgs(byte[] payload, int type)
            : base()
        {
            Payload = payload;
            if (payload != null)
                PayloadStream = new MemoryStream(payload);

            Type = type;
        }


        /// <summary>
        /// Gets a memory stream over the payload.
        /// </summary>
        public MemoryStream PayloadStream { get; private set; }
        /// <summary>
        /// Gets the array of bytes that make up the data payload.
        /// </summary>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Gets the ID associated with this data package.
        /// </summary>
        public Int32 Type { get; private set; }
    }
}
