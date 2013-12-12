using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Server.ClientDisconnected event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class ServerClientDisconnectedEventArgs
        : ExceptionCarryingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ServerClientDisconnectedEventArgs with the specified client object, client ID and exception.
        /// </summary>
        /// <param name="id">The ID of the client.</param>
        /// <param name="client">The client object.</param>
        /// <param name="x">The exception carried by the event.</param>
        public ServerClientDisconnectedEventArgs(int id, BaseClient client, Exception x)
            : base(x)
        {
            ID = id;
            Client = client;
        }

        /// <summary>
        /// Initializes a new instance of the vProto.Events.ServerClientDisconnectedEventArgs with the specified client handler object and exception.
        /// <para>The client ID will be extracted from the client handler object.</para>
        /// </summary>
        /// <param name="client">The client handler object.</param>
        /// <param name="x">The exception carried by the event.</param>
        public ServerClientDisconnectedEventArgs(BaseClient client, Exception x)
            : this(client.ID, client, x)
        { }


        /// <summary>
        /// Gets the ID of the client.
        /// </summary>
        public Int32 ID { get; private set; }
        /// <summary>
        /// Gets the client object.
        /// </summary>
        public BaseClient Client { get; private set; }
    }
}
