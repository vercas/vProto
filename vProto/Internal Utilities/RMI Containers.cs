using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [System.Runtime.Serialization.OptionalField]
        public object Return;
        [System.Runtime.Serialization.OptionalField]
        public object[] Args;   //  Used for out parameters. :C
        [System.Runtime.Serialization.OptionalField]
        public Exception Exception;

        public RmiReturn(object a, object[] b, Exception x)
        {
            Return = a;
            Args = b;
            Exception = x;
        }
    }
}
