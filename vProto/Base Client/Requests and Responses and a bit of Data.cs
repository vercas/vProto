using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto
{
    using Internal_Utilities;
    using Events;
    using Packages;

    //public delegate void ResponseHandler(BaseClient client, byte[] payload);

    partial class BaseClient
    {
        Dictionary<short, StoredRequest> PendingRequests = new Dictionary<short, StoredRequest>();
        Dictionary<short, StoredRequest> PendingResponses = new Dictionary<short, StoredRequest>();



        short reqCounter = short.MinValue;

        private short __getNewRequestID()
        {
            /* Fetches a merry new request ID that has the least chances of being used at the time of request.
             * Whether the other side uses it or not is too difficult to determine and too slow.
             */

            if (reqCounter == short.MaxValue)
            {
                for (short s = short.MinValue; s <= short.MaxValue; s++)
                    if (!PendingRequests.ContainsKey(s))
                        return reqCounter = s;

                throw new Exception("How on Earth can you have 65536 pending outgoing requests..?" + Environment.NewLine + "You really need to fix stuff up.");
            }
            else
            {
                while (PendingRequests.ContainsKey(reqCounter))
                    reqCounter++;

                return reqCounter++;
            }
        }



        protected virtual void OnInternalRequestReceived(Package pack)
        {
            var timeout = new TimeSpan(0, 0, 0, 0, pack.Header.RequestTimeout * 10) - Ping;

            var res = new Response(this, pack.Header.IDTop, pack.Header.IDBottom, pack.Payload, timeout, DateTime.Now);

            var sr = new StoredRequest() { ID = pack.Header.IDTop, res = res };

            sr.timeouttimer = new System.Threading.Timer(new System.Threading.TimerCallback(delegate(object state)
            {
                if (!res.Sent)
                {
                    PendingResponses.Remove(sr.ID);
                    res.DeclareTimeout();
                    sr.timeouttimer.Dispose();
                }
            }), null, timeout, System.Threading.Timeout.InfiniteTimeSpan);//*/

            PendingResponses.Add(sr.ID, sr);

            //Console.WriteLine("Received request: {0} {1}", pack.Header.IDTop, pack.Header.IDBottom);

            OnRequestReceived(new RequestReceivedEventArgs(res));
        }

        protected virtual void OnInternalRequestSent(Package pack)
        {
            var req = pack.State as Request;

            var sr = new StoredRequest() { ID = pack.Header.IDTop, req = req };

            sr.timeouttimer = new System.Threading.Timer(new System.Threading.TimerCallback(delegate(object state)
            {
                if (!req.Responded)
                {
                    PendingRequests.Remove(sr.ID);
                    req.DeclareTimeout();
                    sr.timeouttimer.Dispose();
                }
            }), null, ((int)pack.Header.RequestTimeout) * 10, System.Threading.Timeout.Infinite);

            //  I would gladly declare this with the other properties of 'sr' in the constructor, if only it didn't access 'sr' directly.

            PendingRequests.Add(sr.ID, sr);

            //Console.WriteLine("Sent request: {0} {1}", pack.Header.IDTop, pack.Header.IDBottom);
        }



        protected virtual void OnInternalResponseReceived(Package pack)
        {
            /* Getting unmatching requests might not be a bad thing all the time.
             * For example, requests might time out quickly and the responses might arrive later.
             * If this is the case, the package must simply be ignored.
             * 
             * But in debug mode, this event will be signaled.
             * Timeouts should be a sign that something wrong happened.
             * Wrong things shouldn't happen.
             * Things that shouldn't happen should be debugged.
             */

            //Console.WriteLine("Received response: {0} {1}", pack.Header.IDTop, pack.Header.IDBottom);

            var id = pack.Header.IDTop;

            if (PendingRequests.ContainsKey(id))
            {
                var sr = PendingRequests[id];
                PendingRequests.Remove(id);
                sr.timeouttimer.Dispose();

                //if (!sr.req.Disposed) //  What on Earth was I thinking?!

                if (sr.req.AwaitingResult)
                    sr.req.DeclareResponded(pack.Payload);
            }
#if DEBUG
            else
            {
                System.Diagnostics.Debug.WriteLine("Got bogus response of id {0} and type {1}. Request might have timed out?", pack.Header.IDTop, pack.Header.IDBottom);
            }
#endif
        }

        protected virtual void OnInternalResponseSent(Package pack)
        {
            //Console.WriteLine("Sent response: {0} {1}", pack.Header.IDTop, pack.Header.IDBottom);

            var id = pack.Header.IDTop;

            if (PendingResponses.ContainsKey(id))
            {
                var sr = PendingResponses[id];
                PendingResponses.Remove(id);
                sr.timeouttimer.Dispose();

                //  The response will be disposed and declared sent when this is invoked.
            }
#if DEBUG
            else
            {
                System.Diagnostics.Debug.WriteLine("Sent bogus response of id {0} and type {1}. Shouldn't happen...", pack.Header.IDTop, pack.Header.IDBottom);
            }
#endif
        }



        protected virtual void OnInternalRequestSendFailed(Package pack, Exception x)
        {
            var id = pack.Header.IDTop;

            if (PendingRequests.ContainsKey(id))
            {
                var sr = PendingRequests[id];
                PendingRequests.Remove(id);
                sr.timeouttimer.Dispose();

                //if (!sr.req.Disposed) //  What on Earth was I thinking?!

                if (sr.req.AwaitingResult)
                    sr.req.DeclareFailure(x, true);
            }
#if DEBUG
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to send request {0} of type {1}!", pack.Header.IDTop, pack.Header.IDBottom);
            }
#endif
        }

        protected virtual void OnInternalResponseSendFailed(Package pack, Exception x)
        {
            /*var id = pack.Header.IDTop;

            if (PendingRequests.ContainsKey(id))
            {
                var sr = PendingRequests[id];
                PendingRequests.Remove(id);
                sr.timeouttimer.Dispose();

                //if (!sr.req.Disposed) //  What on Earth was I thinking?!

                if (sr.req.Pending)
                    sr.req.DeclareFailure(x, true);
            }*/
#if DEBUG
            //else
            {
                System.Diagnostics.Debug.WriteLine("Failed to send response {0} of type {1}!", pack.Header.IDTop, pack.Header.IDBottom);
            }
#endif
        }

        protected virtual void OnInternalResponseReceiveFailed(Package pack, Exception x)
        {
            var id = pack.Header.IDTop;

            if (PendingRequests.ContainsKey(id))
            {
                var sr = PendingRequests[id];
                PendingRequests.Remove(id);
                sr.timeouttimer.Dispose();

                //if (!sr.req.Disposed) //  What on Earth was I thinking?!

                if (sr.req.AwaitingResult)
                    sr.req.DeclareFailure(x, false);
            }
#if DEBUG
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to receive response {0} of type {1}!", pack.Header.IDTop, pack.Header.IDBottom);
            }
#endif
        }



        /// <summary>
        /// Sends a shoot-and-forget payload.
        /// </summary>
        /// <param name="payload">The data to send.</param>
        /// <param name="id">An optional ID to associate with the data; useful for identifying its purpose.</param>
        /// <param name="cbk">An optional action to execute after the package has been sent successfully.</param>
        public void SendData(byte[] payload, int id = 0, Action cbk = null)
        {
            _SendPack(payload, new PackageHeader() { ID = id, Type = PackageType.Data }, cbk);
        }

        /// <summary>
        /// Sends a shoot-and-forget payload.
        /// <para>Reading from the stream happens synchronously.</para>
        /// </summary>
        /// <param name="payload">The stream from which to extract the data.</param>
        /// <param name="len">An optional amount of bytes to send from the stream starting from the current position.<para>Use null to send all of it.</para></param>
        /// <param name="id">An optional ID to associate with the data; useful for identifying its purpose.</param>
        /// <param name="cbk">An optional action to execute after the package has been sent successfully.</param>
        public void SendData(System.IO.Stream payload, int? len = null, int id = 0, Action cbk = null)
        {
            _SendPack(payload, new PackageHeader() { ID = id, Type = PackageType.Data }, len, cbk);
        }



        /// <summary>
        /// Creates, assigns and returns a new request object.
        /// </summary>
        /// <param name="type">An optional number to identify the type of request. Can be specified later.</param>
        /// <param name="payload">The data to send; mandatory but may be specified later.</param>
        /// <returns>The request object.</returns>
        public Request CreateRequest(short? type = null, byte[] payload = null)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            var req = new Request(this, __getNewRequestID());

            if (type != null && type.HasValue)
                req.SetType(type.Value);

            if (payload != null)
                req.SetPayload(payload);

            return req;
        }
    }
}
