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

        internal protected BaseClient CurrentClient
        {
            get
            {
                return client.Value;
            }
        }
    }
}
