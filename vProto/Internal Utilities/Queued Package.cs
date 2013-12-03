using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto.Internal_Utilities
{
    internal class QueuedPackage
    {
        public byte[] Data;
        public AsyncCallback AsynchronousCallback;
        public vProto.Packages.Package PackageObject;

        public QueuedPackage(byte[] pl, AsyncCallback cbk, vProto.Packages.Package st)
        {
            Data = pl;
            AsynchronousCallback = cbk;
            PackageObject = st;
        }
    }
}
