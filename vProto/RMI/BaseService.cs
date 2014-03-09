#if NET_4_0_PLUS
using System;
using System.Threading;

namespace vProto.RMI
{
    /// <summary>
    /// Optional base class for vProto RMI services.
    /// </summary>
    public abstract class BaseService
    {
        internal ThreadLocal<BaseClient> client = new ThreadLocal<BaseClient>(() => null);

        /// <summary>
        /// Gets the vProto.BaseClient object corresponding to the client which invoked the current service method execution.
        /// </summary>
        internal protected BaseClient CurrentClient
        {
            get
            {
                return client.Value;
            }
        }
    }
}
#endif
