using System;
using System.Collections.Generic;

namespace vProto.Collections
{
    using Events;

    using IDictType = IDictionary<short, Events.ClientEventHandler<Events.RequestReceivedEventArgs>>;
#if NET_3_5_PLUS
    using ListType = SortedList<short, Events.ClientEventHandler<Events.RequestReceivedEventArgs>>;
#else
    using ListType = Dictionary<short, Events.ClientEventHandler<Events.RequestReceivedEventArgs>>;
#endif

    /// <summary>
    /// A collection of handlers for specific request types.
    /// </summary>
    public class RequestHandlerCollection
    {
        internal ListType list = new ListType();
        private IDictType inner = null;

        internal RequestHandlerCollection parent = null;    //  Mainly a memory-saving mechanism.

        private object syncRoot = null;



        internal RequestHandlerCollection(RequestHandlerCollection parent = null)
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



        /// <summary>
        /// Executes the handlers for a specified request type.
        /// </summary>
        /// <param name="key">The request type.</param>
        /// <param name="client">Client on which to execute handler; provided in event delegate.</param>
        /// <param name="e">Desired event arguments.</param>
        public void ExecuteHandler(short key, BaseClient client, RequestReceivedEventArgs e)
        {
            ClientEventHandler<RequestReceivedEventArgs> value;

            if (TryGetValue(key, out value))
            {
                value.Invoke(client, e);
            }
        }



        /// <summary>
        /// Assigns the handler to a request type.
        /// </summary>
        /// <param name="key">The request type.</param>
        /// <param name="value">Handler.</param>
        public void Add(short key, ClientEventHandler<RequestReceivedEventArgs> value)
        {
            lock (syncRoot)
                inner.Add(key, value); 
        }

        /// <summary>
        /// Determines whether there is a handler for the specified request type.
        /// </summary>
        /// <param name="key">The request type.</param>
        /// <returns></returns>
        public bool ContainsKey(short key)
        {
            lock (syncRoot)
                if (inner.ContainsKey(key))
                    return true;

            if (parent != null)
                return parent.ContainsKey(key);

            return false;
        }

        /// <summary>
        /// Removes the handler for the specified request type.
        /// </summary>
        /// <param name="key">The request type.</param>
        /// <returns>True if there was a handler for the specified request type; otherwise false.</returns>
        public bool Remove(short key)
        {
            lock (syncRoot)
                return inner.Remove(key);
        }

        /// <summary>
        /// Gets the handler associated with the specified request type.
        /// </summary>
        /// <param name="key">The request type.</param>
        /// <param name="value">The handler of the specified request type, if found.</param>
        /// <returns></returns>
        public bool TryGetValue(short key, out ClientEventHandler<RequestReceivedEventArgs> value)
        {
            lock (syncRoot)
                if (inner.TryGetValue(key, out value))
                    return true;

            if (parent != null)
                return parent.TryGetValue(key, out value);

            return false;
        }



        /// <summary>
        /// Gets or sets the handler associated with the given request type.
        /// </summary>
        /// <param name="key">The request type.</param>
        /// <returns>Handler associated with given request type.</returns>
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



        /// <summary>
        /// Clears the collection, leaving all request types handler-free.
        /// </summary>
        public void Clear()
        {
            lock (syncRoot)
                inner.Clear();
        }

        /// <summary>
        /// Gets the number of request types with associated handlers.
        /// </summary>
        public int Count
        {
            //  I doubt the need to lock on this one.
            get { return inner.Count; }
        }
    }
}
