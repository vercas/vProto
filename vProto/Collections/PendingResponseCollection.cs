using System;
using System.Collections.Generic;
using System.Threading;

namespace vProto.Collections
{
    using Internals;

    class PendingResponseCollection : ICollection<Response>
    {
        internal Dictionary<short, StoredRequest> dict = new Dictionary<short, StoredRequest>();



        internal PendingResponseCollection()
        {
            //  Nope.
        }



        public void Add(Response item)
        {
            if (dict.ContainsKey(item.id))
                throw new InvalidOperationException("Duplicate ID..?");

            Timer timeouttimer = null;

            timeouttimer = new Timer(new TimerCallback(delegate(object state)
            {
                if (!item.Sent)
                {
                    dict.Remove(item.id);
                    item.DeclareTimeout();
                }

                //  Trying here just in case it was disposed of already.

                try
                {
                    timeouttimer.Dispose();
                }
                catch { }
            }), null, item.Timeout, BaseClient.InfiniteTimeSpan);

            dict.Add(item.id, new StoredRequest() { ID = item.id, res = item, timeouttimer = timeouttimer });
        }

        public bool DeclareSent(short id)
        {
            if (dict.ContainsKey(id))
            {
                var sr = dict[id];
                dict.Remove(id);
                sr.timeouttimer.Dispose();

                return true;
            }

            return false;
        }

        public bool DeclareFailure(short id, Exception x, bool sending = true)
        {
            if (dict.ContainsKey(id))
            {
                var sr = dict[id];
                dict.Remove(id);
                sr.timeouttimer.Dispose();

                //if (!sr.req.Disposed) //  What on Earth was I thinking?!

                if (sr.req.AwaitingResult)
                    sr.req.DeclareFailure(x, sending);

                return true;
            }

            return false;
        }



        public void Clear()
        {
            foreach (var kv in dict)
            {
                DeclareFailure(kv.Key, new OperationCanceledException("Clearing of pending request collection."));
            }
        }

        public bool Contains(Response item)
        {
            return dict.ContainsKey(item.id);
        }

        public void CopyTo(Response[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Response item)
        {
            return dict.Remove(item.id);
        }

        public IEnumerator<Response> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
