using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto
{
    using Internals;
    using Events;
    using Packages;
    using Collections;

    partial class BaseClient
    {
        PendingRequestCollection PendingRequests = new PendingRequestCollection();
        PendingResponseCollection PendingResponses = new PendingResponseCollection();

        PendingRequestCollection PendingInternalRequests = new PendingRequestCollection();
        PendingResponseCollection PendingInternalResponses = new PendingResponseCollection();



        protected virtual void OnInternalRequestReceived(Package pack)
        {
            var res = new Response(this, pack.Header.IDTop, pack.Header.IDBottom, pack.Payload, new TimeSpan(0, 0, 0, 0, pack.Header.RequestTimeout * 10), DateTime.Now) { isInternal = pack.Header.Type == PackageType.InternalRequest };

            if (pack.Header.Type == PackageType.InternalRequest)
            {
                PendingInternalResponses.Add(res);

                var e = new RequestReceivedEventArgs(res);

                InternalRequestHandlers.ExecuteHandler(pack.Header.IDBottom, this, e);
            }
            else
            {
                PendingResponses.Add(res);

                var e = new RequestReceivedEventArgs(res);

                OnRequestReceived(e);
                RequestHandlers.ExecuteHandler(pack.Header.IDBottom, this, e);
            }

            //Console.WriteLine("Received request: {0} {1}", pack.Header.IDTop, pack.Header.IDBottom);
        }

        protected virtual void OnInternalRequestSent(Package pack)
        {
            if (pack.Header.Type == PackageType.InternalRequest)
                PendingInternalRequests.Add(pack.State as Request);
            else
                PendingRequests.Add(pack.State as Request);

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

            bool res;

            if (pack.Header.Type == PackageType.InternalResponse)
                res = PendingInternalRequests.DeclareResponse(pack.Header.IDTop, pack.Payload);
            else
                res = PendingRequests.DeclareResponse(pack.Header.IDTop, pack.Payload);

            if (res)
            {
                // All good.
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

            bool res;

            if (pack.Header.Type == PackageType.InternalResponse)
                res = PendingInternalResponses.DeclareSent(pack.Header.IDTop);
            else
                res = PendingResponses.DeclareSent(pack.Header.IDTop);

            if (res)
            {
                // yay?
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
            bool res;

            if (pack.Header.Type == PackageType.InternalResponse)
                res = PendingInternalRequests.DeclareFailure(pack.Header.IDTop, x, true);
            else
                res = PendingRequests.DeclareFailure(pack.Header.IDTop, x, true);

            if (res)
            {
                // All good.
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
            bool res;

            if (pack.Header.Type == PackageType.InternalResponse)
                res = PendingInternalRequests.DeclareFailure(pack.Header.IDTop, x, false);
            else
                res = PendingRequests.DeclareFailure(pack.Header.IDTop, x, false);

            if (res)
            {
                // All good.
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

            var req = new Request(this, PendingRequests.GetFreeID());

            if (type != null && type.HasValue)
                req.SetType(type.Value);

            if (payload != null)
                req.SetPayload(payload);

            return req;
        }

        
        internal Request __createInternalRequest(InternalRequestType type)
        {
            return new Request(this, PendingInternalRequests.GetFreeID()) { isInternal = true, Type = (short)type };
        }
    }
}
