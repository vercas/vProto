using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto.Collections
{
    using Events;

    using IDictType = IDictionary<short, Events.ClientEventHandler<Events.RequestReceivedEventArgs>>;
    using ListType = SortedList<short, Events.ClientEventHandler<Events.RequestReceivedEventArgs>>;

    public class RequestHandlerCollection
    {
        internal ListType list = new ListType();
        private IDictType inner = null;

        internal RequestHandlerCollection parent = null;    //  Mainly a memory-saving mechanism.

        private object syncRoot = null;



        public RequestHandlerCollection(RequestHandlerCollection parent = null)
        {
            inner = list;

            var temp = parent;

            while (temp != null)
            {
                if (temp == this)
                    throw new Exception("Circular reference!");
                //  Anti-self-idiocy
                temp = temp.parent;
            }

            this.parent = parent;

            syncRoot = ((System.Collections.ICollection)list).SyncRoot;
        }



        public void ExecuteHandler(short key, BaseClient client, RequestReceivedEventArgs e)
        {
            ClientEventHandler<RequestReceivedEventArgs> value;

            if (TryGetValue(key, out value))
            {
                value.Invoke(client, e);
            }
        }



        public void Add(short key, ClientEventHandler<RequestReceivedEventArgs> value)
        {
            lock (syncRoot)
                inner.Add(key, value); 
        }

        public bool ContainsKey(short key)
        {
            lock (syncRoot)
                if (inner.ContainsKey(key))
                    return true;

            if (parent != null)
                return parent.ContainsKey(key);

            return false;
        }

        public bool Remove(short key)
        {
            lock (syncRoot)
                return inner.Remove(key);
        }

        public bool TryGetValue(short key, out ClientEventHandler<RequestReceivedEventArgs> value)
        {
            lock (syncRoot)
                if (inner.TryGetValue(key, out value))
                    return true;

            if (parent != null)
                return parent.TryGetValue(key, out value);

            return false;
        }



        public ClientEventHandler<RequestReceivedEventArgs> this[short key]
        {
            get
            {
                lock (syncRoot)
                    if (inner.ContainsKey(key))
                        return inner[key];

                if (parent != null)
                    return parent[key];

                throw new KeyNotFoundException();
            }
            set
            {
                lock (syncRoot)
                    inner[key] = value;
            }
        }



        public void Clear()
        {
            lock (syncRoot)
                inner.Clear();
        }

        public int Count
        {
            //  I doubt the need to lock on this one.
            get { return inner.Count; }
        }
    }
}
