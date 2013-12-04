using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace vProto
{
    using vProto.Events;

    partial class BaseServer
    {
        protected abstract bool StartListening();
    }
}
