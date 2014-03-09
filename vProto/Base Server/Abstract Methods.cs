using System;

namespace vProto
{
    using Events;

    partial class BaseServer
    {
        /// <summary>
        /// When overriden in a derived class, attempts to start listening for client connections with previously specified settings.
        /// </summary>
        protected abstract bool StartListening();
    }
}
