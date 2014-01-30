using System;

namespace vProto.Internals
{
    using Packages;

    internal class QueuedPackage
    {
        public byte[] Data;
        public AsyncCallback AsynchronousCallback;
        public Package PackageObject;

        public QueuedPackage(byte[] pl, AsyncCallback cbk, Package st)
        {
            Data = pl;
            AsynchronousCallback = cbk;
            PackageObject = st;
        }
    }
}
