using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto.Internal_Utilities
{
    internal class StoredRequest
    {
        public short ID;
        public OutboundRequest req;
        public System.Threading.Timer timeouttimer;
    }
}
