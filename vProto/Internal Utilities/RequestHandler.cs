using System;

namespace vProto.Internals
{
    using Collections;
    using Events;
    using Packages;

    public abstract class _RequestHandler
    {
        protected RequestHandlerCollection RequestHandlers = new RequestHandlerCollection();
        protected RequestHandlerCollection InternalRequestHandlers = new RequestHandlerCollection();



        /// <summary>
        /// Adds a handler delegate for a specific request type.
        /// </summary>
        /// <param name="requestType">The type of request to handle.</param>
        /// <param name="handler">The delegate which will handle the request.</param>
        public void AddRequestHandler(short requestType, ClientEventHandler<RequestReceivedEventArgs> handler)
        {
            RequestHandlers.Add(requestType, handler);
        }

        /// <summary>
        /// Removes the handler for the specified request type.
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns>True if the handler was found and removed; otherwise false.</returns>
        public bool RemoveRequestHandler(short requestType)
        {
            return RequestHandlers.Remove(requestType);
        }

        /// <summary>
        /// Determines whether a handler exists for a specified request type or not.
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns>True if exists; otherwise false.</returns>
        public bool ContainsRequestHandler(short requestType)
        {
            return RequestHandlers.ContainsKey(requestType);
        }



        //  Allow overriding classes to register their own internal requests.

        /// <summary>
        /// Adds a handler delegate for a specific request type.
        /// </summary>
        /// <param name="requestType">The type of request to handle.</param>
        /// <param name="handler">The delegate which will handle the request.</param>
        protected void AddInternalRequestHandler(short requestType, ClientEventHandler<RequestReceivedEventArgs> handler)
        {
            RequestHandlers.Add(requestType, handler);
        }

        /// <summary>
        /// Removes the handler for the specified request type.
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns>True if the handler was found and removed; otherwise false.</returns>
        protected bool RemoveInternalRequestHandler(short requestType)
        {
            return RequestHandlers.Remove(requestType);
        }

        /// <summary>
        /// Determines whether a handler exists for a specified request type or not.
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns>True if exists; otherwise false.</returns>
        protected bool ContainsInternalRequestHandler(short requestType)
        {
            return RequestHandlers.ContainsKey(requestType);
        }



        internal void __addInternalRequestHandler(InternalRequestType requestType, ClientEventHandler<RequestReceivedEventArgs> handler)
        {
            InternalRequestHandlers.Add((short)requestType, handler);
        }

        internal bool __removeInternalRequestHandler(InternalRequestType requestType)
        {
            return InternalRequestHandlers.Remove((short)requestType);
        }

        internal bool __containsInternalRequestHandler(InternalRequestType requestType)
        {
            return InternalRequestHandlers.ContainsKey((short)requestType);
        }



        protected virtual void __registerDefaultInternalRequestHandlers()
        {

        }
    }
}
