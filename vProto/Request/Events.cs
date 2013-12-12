using System;

namespace vProto
{
    using Events;

    partial class Request
    {
        public event RequestEventHandler<ResponseReceivedEventArgs> ResponseReceived;
        public event RequestEventHandler RequestSent;
        public event RequestEventHandler RequestAborted;
        public event RequestEventHandler RequestTimeout;
        public event RequestEventHandler<RequestFailureEventArgs> RequestFailure;


        /// <summary>
        /// Adds a delegate to the ResponseReceived event of this request.
        /// </summary>
        /// <param name="handler">The handler to add to the event.</param>
        /// <returns>The current request object.</returns>
        public Request AddResponseReceivedHandler(RequestEventHandler<ResponseReceivedEventArgs> handler)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

            if (handler == null)
                throw new ArgumentNullException("handler");

            ResponseReceived += handler;

            return this;
        }

        /// <summary>
        /// Adds a delegate to the RequestSent event of this request.
        /// </summary>
        /// <param name="handler">The handler to add to the event.</param>
        /// <returns>The current request object.</returns>
        public Request AddSentHandler(RequestEventHandler handler)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

            if (handler == null)
                throw new ArgumentNullException("handler");

            RequestSent += handler;

            return this;
        }

        /// <summary>
        /// Adds a delegate to the RequestAborted event of this request.
        /// </summary>
        /// <param name="handler">The handler to add to the event.</param>
        /// <returns>The current request object.</returns>
        public Request AddAbortedHandler(RequestEventHandler handler)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

            if (handler == null)
                throw new ArgumentNullException("handler");

            RequestAborted += handler;

            return this;
        }

        /// <summary>
        /// Adds a delegate to the RequestTimeout event of this request.
        /// </summary>
        /// <param name="handler">The handler to add to the event.</param>
        /// <returns>The current request object.</returns>
        public Request AddTimeoutHandler(RequestEventHandler handler)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

            if (handler == null)
                throw new ArgumentNullException("handler");

            RequestTimeout += handler;

            return this;
        }

        /// <summary>
        /// Adds a delegate to the RequestFailure event of this request.
        /// </summary>
        /// <param name="handler">The handler to add to the event.</param>
        /// <returns>The current request object.</returns>
        public Request AddFailureHandler(RequestEventHandler<RequestFailureEventArgs> handler)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot change a disposed request!");

            if (handler == null)
                throw new ArgumentNullException("handler");

            RequestFailure += handler;

            return this;
        }


        /*protected virtual*/ void OnResponseReceived(ResponseReceivedEventArgs e)
        {
            RequestEventHandler<ResponseReceivedEventArgs> handler = ResponseReceived;

            if (handler != null)
            {
                handler(this, client, e);
            }
        }

        /*protected virtual*/ void OnRequestSent(EventArgs e)
        {
            RequestEventHandler handler = RequestSent;

            if (handler != null)
            {
                handler(this, client, e);
            }
        }

        /*protected virtual*/ void OnRequestAborted(EventArgs e)
        {
            RequestEventHandler handler = RequestAborted;

            if (handler != null)
            {
                handler(this, client, e);
            }
        }

        /*protected virtual*/ void OnRequestTimeout(EventArgs e)
        {
            RequestEventHandler handler = RequestTimeout;

            if (handler != null)
            {
                handler(this, client, e);
            }
        }

        /*protected virtual*/
        void OnRequestFailure(RequestFailureEventArgs e)
        {
            RequestEventHandler<RequestFailureEventArgs> handler = RequestFailure;

            if (handler != null)
            {
                handler(this, client, e);
            }
        }
    }
}
