using System;
using System.IO;

namespace vProto
{
    partial class Response
    {
        MemoryStream reqstr = null;
        byte[] reqarr = null;



        /// <summary>
        /// Gets a memory stream over the request's payload.
        /// </summary>
        public MemoryStream RequestPayloadStream { get { return reqstr; } }

        /// <summary>
        /// Gets an array of bites containing the payload of the request.
        /// </summary>
        public Byte[] RequestPayload { get { return reqarr; } }
    }
}
