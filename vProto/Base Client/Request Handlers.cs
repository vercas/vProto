using System;
using System.Collections.Generic;

namespace vProto
{
    using Events;
    using Packages;

    partial class BaseClient
    {
        protected override void __registerDefaultInternalRequestHandlers()
        {
            base.__registerDefaultInternalRequestHandlers();

            __addInternalRequestHandler(InternalRequestType.RMI, _handleRmiRequest);
        }
    }
}
