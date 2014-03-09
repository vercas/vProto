namespace vProto.Packages
{
    /// <summary>
    /// Represents all the types of packages used by the protocol.
    /// </summary>
    public enum PackageType
        : byte
    {
        /// <summary>
        /// A reserved package type value.
        /// </summary>
        Reserved = 0,   //  For God knows what, but I like number 0 so I won't use it. >_>

        /// <summary>
        /// A package which demands a response.
        /// </summary>
        Request = 1,
        /// <summary>
        /// A response to a demanding package.
        /// </summary>
        Response = 2,
        /// <summary>
        /// Shoot-and-forget package.
        /// </summary>
        Data = 3,

        /// <summary>
        /// Used to check connection health and estimate latency.
        /// </summary>
        HeartbeatRequest = 4,
        /// <summary>
        /// Used to check connection health and estimate latency.
        /// </summary>
        HeartbeatResponse = 5,

        /// <summary>
        /// Internal requests, usually wrapping other functionalities.
        /// </summary>
        InternalRequest = 6,
        /// <summary>
        /// Internal responses, usually wrapping other functionalities.
        /// </summary>
        InternalResponse = 7,

        /// <summary>
        /// Signals the connection of a peer to the server.
        /// </summary>
        PeerConnected = 8,
        /// <summary>
        /// Signals the disconnection of a peer from the server.
        /// </summary>
        PeerDisconnected = 9,
    }
}
