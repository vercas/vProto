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
        /// Initializes a new instance of the vProto.Events.RequestReceivedEventArgs with the specified response object.
        /// </summary>
        /// <param name="response">The object used to read and respond to the request.</param>
        public RequestReceivedEventArgs(Response response)
            : base()
        {
            Response = response;
        }


        /// <summary>
        /// Gets the object used to read the request and respond to it.
        /// </summary>
        public Response Response { get; private set; }
    }
}
