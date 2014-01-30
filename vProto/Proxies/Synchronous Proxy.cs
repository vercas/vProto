using System;
using System.IO;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace vProto.Proxies
{
    using Internals;
    using Packages;

    public class SynchronousProxy<TService> : RealProxy, INamedProxy, ITypedProxy<TService>
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



        public override IMessage Invoke(IMessage msg)
        {
            if (!client.IsConnected)
                return null;

            if (msg is IMethodCallMessage)
            {
                var mcm = msg as IMethodCallMessage;

                var req = client.__createInternalRequest(InternalRequestType.RMI);

                //  Method calls may take up quite a lot of space.
                //  4 Kibibytes are allocated by default in the buffer.

                /*var payload = new MemoryStream(4096);

                using (var bw = new BinaryWriter(payload, Encoding.UTF8, true))
                {
                    bw.Write(name);

                    bw.Write(mcm.MethodName);

                    var data = BinarySerialization.Serialize(mcm.InArgs);

                    bw.Write(data.Length);
                    bw.Write(data);

                    System.Diagnostics.Debug.WriteLine("RMI: {0}", mcm.Uri);
                }

                return Handle(req.SetPayload(payload, -1, 0, SeekOrigin.Begin), mcm);//*/

                //return Handle(req.SetPayload(BinarySerialization.Serialize(mcmsg)), mcmsg);
                //  Is this a sin?

                return Handle(req.SetPayload(BinarySerialization.Serialize(new RmiCall(Type, mcm.MethodName, mcm.Args))), mcm);
            }

            return null;
        }

        protected virtual IMessage Handle(Request req, IMethodCallMessage mcm)
        {
            var task = req.SendAsync();
            byte[] res;

            try
            {
                task.Wait();
                res = task.Result;
            }
            catch (Exception x)
            {
                return new ReturnMessage(x, mcm);
            }

            var ret = BinarySerialization.Deserialize<RmiReturn>(res);

            if (ret.Exception == null)
                return new ReturnMessage(ret.Return, new object[0], 0, mcm.LogicalCallContext, mcm);
            else
                return new ReturnMessage(ret.Exception, mcm);
        }
    }
}
