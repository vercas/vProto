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
        public System.Threading.Timer timeouttimer;

        public Request req;
        public Response res;
    }
}
