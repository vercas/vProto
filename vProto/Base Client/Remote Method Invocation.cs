using System;
using System.Collections.Generic;
using System.Reflection;

namespace vProto
{
    using Events;
    using Internals;
    using Proxies;

    partial class BaseClient
    {
        private Dictionary<Type, object> rmiServices = new Dictionary<Type, object>();
        private Dictionary<Type, Tuple<object, INamedProxy>> rmiClients = new Dictionary<Type, Tuple<object, INamedProxy>>();



        /// <summary>
        /// Registers a Remote Method Invocation service with the client.
        /// <para>A RMI service is an object whose methods can be accessed remotely over the connection.</para>
        /// </summary>
        /// <typeparam name="TService">The service interface.</typeparam>
        /// <param name="obj">An instance of the service object which implements the given interface.</param>
        /// <returns>False if the service already exists; otherwise true</returns>
        public bool RegisterRmiService<TService>(TService obj)
            where TService : class
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Client object is disposed!");

            if (!typeof(TService).IsInterface)
                throw new ArgumentException("Service interface parameter must be an interface!", "TService");

            if (obj == null)
                throw new ArgumentNullException("obj", "The given service object must not be nil!");

            var type = typeof(TService);
            object o;

            if (rmiServices.TryGetValue(type, out o))
            {
                return false;
            }
            else
            {
                rmiServices.Add(type, obj);

                return true;
            }
        }

        /// <summary>
        /// Acquires a proxy object to a Remote Method Invocation service on the other side.
        /// <para>The proxy is used to remotely access methods of the service object over the connection.</para>
        /// </summary>
        /// <typeparam name="TService">The service interface.</typeparam>
        /// <returns>RMI service proxy object</returns>
        public TService ProxyRmiService<TService>()
            where TService : class
        {
            Tuple<object, INamedProxy> res = null;

            if (rmiClients.TryGetValue(typeof(TService), out res))
            {
                return res.Item1 as TService;
            }
            else
            {
                var proxy = new SynchronousProxy<TService>(this);

                rmiClients.Add(typeof(TService), new Tuple<object, INamedProxy>(proxy.Object, proxy));

                return proxy.Object;
            }
        }



        private void _handleRmiRequest(BaseClient sender, RequestReceivedEventArgs e)
        {
            var call = BinarySerialization.Deserialize<RmiCall>(e.Response.RequestPayload);

            RmiReturn ret = null;

            //Console.WriteLine("RMI Call reuqest:\n    {0}\n    {1}\n    {2} args", call.Interface, call.Method, call.Args.Length);

            object o;

            if (rmiServices.TryGetValue(call.Interface, out o))
            {
                try
                {
                    /*var types = new Type[call.Args.Length];

                    for (int i = 0; i < types.Length; i++)
                        types[i] = call.Args[i].GetType();

                    var result = o.GetType().GetMethod(call.Method, types).Invoke(o, call.Args); // Y U NO WORK? :( */

                    var result = o.GetType().GetMethod(call.Method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod).Invoke(o, call.Args);

                    ret = new RmiReturn(result, call.Args, null);
                }
                catch (Exception x)
                {
                    ret = new RmiReturn(null, null, x);
                }
            }
            else
            {
                ret = new RmiReturn(null, null, new TypeAccessException("There is no service object registered to interface {0}"));
            }

            e.Response.SetPayload(BinarySerialization.Serialize(ret)).Send();
        }
    }
}
