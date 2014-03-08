using System;
using System.Runtime.Serialization;

namespace vProto.Internals
{
    [Serializable]
    internal class RmiCall
    {
        public Type Interface;
        public String Method;
        public object[] Args;

        public RmiCall(Type a, String b, object[] c)
        {
            Interface = a;
            Method = b;
            Args = c;
        }
    }

    [Serializable]
    internal class RmiReturn
    {
        [OptionalField]
        public object Return;
        [OptionalField]
        public object[] Args;   //  Used for out parameters. :C
        [OptionalField]
        public Exception Exception;

        public RmiReturn(object a, object[] b, Exception x)
        {
            Return = a;
            Args = b;
            Exception = x;
        }
    }
}
