using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StateType = vProto.Packages.Package;

namespace vProto.Internal_Utilities
{
    internal class QueuedPackage
    {
        public byte[] payload;
        public AsyncCallback callback;
        public StateType state;

        public QueuedPackage(byte[] pl, AsyncCallback cbk, StateType st)
        {
            payload = pl;
            callback = cbk;
            state = st;
        }
    }
}
