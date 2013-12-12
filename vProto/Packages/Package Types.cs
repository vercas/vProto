namespace vProto.Packages
{
    /// <summary>
    /// Represents all the types of packages used by the protocol.
    /// </summary>
    public enum PackageType
        : uint
    {
        Reserved = 0,   //  For God knows what, but I like number 0 so I won't use it. >_>

        Request = 1,
        Response = 2,
        Data = 3,

        HeartbeatRequest = 4,
        HeartbeatResponse = 5,

        InternalRequest = 6,
        InternalResponse = 7,
    }
}
