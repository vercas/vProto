using System;
using System.Threading;

namespace vProto.Internal_Utilities
{
    internal class StoredRequest
    {
        public short ID;
        public Timer timeouttimer;

        public Request req;
        public Response res;
    }
}
