using System;
using System.IO;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace vProto.RMI
{
    using Internals;
    using Packages;

    /// <summary>
    /// A proxy for synchronous RMI-based communication.
    /// </summary>
    /// <typeparam name="TService">The proxied RMI service interface.</typeparam>
    public class SynchronousProxy<TService>
        : RealProxy, INamedProxy, ITypedProxy<TService>
        where TService : class
    {
        static SynchronousProxy()
        {
            if (!typeof(TService).IsInterface)
                throw new InvalidOperationException("The service of this proxy must be an interface!");

            //  REALLY let people know!
        }



        private TService obj;
        /// <summary>
        /// Gets the proxy object of the service.
        /// </summary>
        public TService Object
        {
            get
            {
                return obj;
            }
        }

        private string name;
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public String Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Gets the proxied interface type.
        /// </summary>
        public Type Type { get; private set; }



        internal BaseClient client;



        internal SynchronousProxy(BaseClient client)
            : base(typeof(TService))
        {
            Type = typeof(TService);

            obj = GetTransparentProxy() as TService;
            name = Type.Name;

            this.client = client;
        }



        /// <summary>
        /// Handles an invocation.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override IMessage Invoke(IMessage msg)
        {
            if (!client.IsConnected)
                return null;

            if (msg is IMethodCallMessage)
            {
                var mcm = msg as IMethodCallMessage;

                var req = client.__createInternalRequest(InternalRequestType.RMI);

                return Handle(req.SetPayload(BinarySerialization.Serialize(new RmiCall(Type, mcm.MethodName, mcm.Args))), mcm);
            }

            return null;
        }

        /// <summary>
        /// Handles a request.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="mcm"></param>
        /// <returns></returns>
        protected virtual IMessage Handle(Request req, IMethodCallMessage mcm)
        {
            byte[] res;

            try
            {
                res = req.SendSynchronous();
            }
            catch (Exception x)
            {
                return new ReturnMessage(x, mcm);
            }

            var ret = BinarySerialization.Deserialize<RmiReturn>(res);

            if (ret.Exception == null)
                return new ReturnMessage(ret.Return, ret.Args, ret.Args.Length, mcm.LogicalCallContext, mcm);
            else
                return new ReturnMessage(ret.Exception, mcm);
        }
    }
}
