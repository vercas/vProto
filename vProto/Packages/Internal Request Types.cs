namespace vProto.Packages
{
    internal enum InternalRequestType
        : short
    {
        RMI = 0,
        Handshake = 1,
        PeerConnected = 2,
        PeerDisconnected = 3
    }
}
