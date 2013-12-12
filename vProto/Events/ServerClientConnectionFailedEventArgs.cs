using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Server.ClientConnectionFailed event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class ServerClientConnectionFailedEventArgs
        : ExceptionCarryingEventArgs
    {
        // / <param name="id">The ID of the client.</param>
        // / <param name="client">The client object.</param>
        
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ServerClientConnectionFailedEventArgs with the specified client object, client ID and exception.
        /// </summary>
        /// <param name="x">The exception carried by the event.</param>
        public ServerClientConnectionFailedEventArgs(/*int id, BaseClient client,*/ Exception x)
            : base(x)
        {
            //ID = id;
            //Client = client;
        }


        /*
        /// <summary>
        /// Gets the ID of the client.
        /// </summary>
        public Int32 ID { get; private set; }
        /// <summary>
        /// Gets the client object.
        /// </summary>
        public BaseClient Client { get; private set; }
        //*/
    }
}
