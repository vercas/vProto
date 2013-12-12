using System;
using System.Collections.Generic;

namespace vProto.Packages
{
    /// <summary>
    /// Wrapper of a protocol package. This class cannot be inherited.
    /// </summary>
    public sealed class Package
    {
        /// <summary>
        /// Header of the package;
        /// </summary>
        public PackageHeader Header { get; private set; }
        /// <summary>
        /// Payload of the package.
        /// </summary>
        public byte[] Payload { get; private set; }



        internal Package(PackageHeader h, byte[] p)
        {
            Header = h;
            Payload = p;
        }



        internal List<Action> Callbacks = new List<Action>();
        internal Object State = null;
    }
}
