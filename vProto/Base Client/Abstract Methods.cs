using System;

namespace vProto
{
    partial class BaseClient
    {
        /// <summary>
        /// When overriden in a derived class, attempts to connect to a previously specified server.
        /// </summary>
        protected abstract void StartConnection();
    }
}
