using System;
using System.Collections.Generic;

namespace vProto
{
    using Events;
    using Packages;

    partial class BaseClient
    {
        /// <summary>
        /// Registers handlers for several internal request types.
        /// </summary>
        protected override void __registerDefaultInternalRequestHandlers()
        {
            base.__registerDefaultInternalRequestHandlers();

            __addInternalRequestHandler(InternalRequestType.RMI, _handleRmiRequest);
            __addInternalRequestHandler(InternalRequestType.Handshake, _handleHandshakeRequest);
        }
    }
}
