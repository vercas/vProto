﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientType = vProto.BaseClient;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.Server.ClientConnected event. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class ServerClientConnectedEventArgs
        : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.ServerClientConnectedEventArgs with the specified client object and client ID.
        /// </summary>
        /// <param name="id">The ID of the client.</param>
        /// <param name="client">The client object.</param>
        public ServerClientConnectedEventArgs(int id, ClientType client)
            : base()
        {
            ID = id;
            Client = client;
        }


        /// <summary>
        /// Gets the ID of the client.
        /// </summary>
        public Int32 ID { get; private set; }
        /// <summary>
        /// Gets the client object.
        /// </summary>
        public ClientType Client { get; private set; }
    }
}
