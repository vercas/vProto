using System;
using System.Collections.Generic;
using System.Threading;

namespace vProto.Collections
{
    using Internal_Utilities;

    internal class PendingRequestCollection : ICollection<Request>
    {
        internal Dictionary<short, StoredRequest> dict = new Dictionary<short, StoredRequest>();



        public void Add(Request item)
        {
            if (dict.ContainsKey(item.id))
                throw new InvalidOperationException("Duplicate ID..?");

            Timer timeouttimer = null;

            timeouttimer = new Timer(new TimerCallback(delegate(object state)
            {
                if (item.AwaitingResult)
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
            }), null, item.Timeout, Timeout.InfiniteTimeSpan);

            dict.Add(item.id, new StoredRequest() { ID = item.id, req = item, timeouttimer = timeouttimer });
        }

        public bool DeclareResponse(short id, byte[] response)
        {
            if (dict.ContainsKey(id))
            {
                var sr = dict[id];
                dict.Remove(id);
                sr.timeouttimer.Dispose();

                //if (!sr.req.Disposed) //  What on Earth was I thinking?!

                if (sr.req.AwaitingResult)
                    sr.req.DeclareResponded(response);

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

        public bool Contains(Request item)
        {
            return dict.ContainsKey(item.id);
        }

        public void CopyTo(Request[] array, int arrayIndex)
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

        public bool Remove(Request item)
        {
            return dict.Remove(item.id);
        }

        public IEnumerator<Request> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }



        short reqCounter = short.MinValue;

        public short GetFreeID()
        {
            if (reqCounter == short.MaxValue)
            {
                for (short s = short.MinValue; s <= short.MaxValue; s++)
                    if (!dict.ContainsKey(s))
                        return reqCounter = s;

                throw new Exception("How on Earth can you have 65536 pending outgoing requests..?" + Environment.NewLine + "You really need to fix stuff up.");
            }
            else
            {
                while (dict.ContainsKey(reqCounter))
                    reqCounter++;

                return reqCounter++;
            }
        }
    }
}
