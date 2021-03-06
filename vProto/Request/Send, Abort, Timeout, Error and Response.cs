﻿using System;

#if NET_4_0_PLUS || NETFX_CORE
using System.Threading.Tasks;
#endif

#if NET_3_5
using System.Threading;
#endif

namespace vProto
{
    using Events;
    using Packages;

    partial class Request
    {
        /// <summary>
        /// Attempts to send the request. This process is asynchronous; unlike SendAsync, this method is not awaitable and will not deliver any result to the caller, only in events.
        /// <para>Upon success, the request is marked as sent.</para>
        /// <para>Whether this fails or not, the request will be disposed of.</para>
        /// </summary>
        /// <returns>The current request object.</returns>
        public Request SendFluent()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot send a disposed request!");

            lock (__syncObject)
            {
                try
                {
                    client._SendPack(str, new PackageHeader() { IDTop = id, IDBottom = Type, Type = this.isInternal ? PackageType.InternalRequest : PackageType.Request, RequestTimeout = __timeout }, null, null, this);
                }
                catch { }

                try
                {
                    str.Dispose();
                }
                catch { }
                finally
                {
                    Disposed = Sent = true;
                }
            }

            return this;
        }

        /// <summary>
        /// Marks the request as aborted and disposed and raises the appropriate event.
        /// </summary>
        /// <returns>The current request object.</returns>
        public Request Abort()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot abort a disposed request!");

            lock (__syncObject)
                try
                {
                    if (str != null)
                        str.Dispose();
                }
                catch { }
                finally
                {
                    Disposed = Aborted = true;

                    OnRequestAborted(new EventArgs());
                }

            return this;
        }

        internal void DeclareTimeout()
        {
            lock (__syncObject)
                try
                {
                    if (str != null)
                        str.Dispose();
                }
                catch { }
                finally
                {
                    Disposed = TimedOut = true;

                    OnRequestTimeout(new EventArgs());
                }
        }

        internal void DeclareFailure(Exception x, bool sending)
        {
            lock (__syncObject)
                try
                {
                    if (str != null)
                        str.Dispose();
                }
                catch { }
                finally
                {
                    Disposed = Failed = true;

                    OnRequestFailure(new RequestFailureEventArgs(x, sending));
                }
        }

        internal void DeclareResponded(byte[] payload)
        {
            lock (__syncObject)
                try
                {
                    if (str != null)
                        str.Dispose();
                }
                catch { }
                finally
                {
                    Disposed = Responded = true;

                    OnResponseReceived(new ResponseReceivedEventArgs(payload));
                }
        }



#if NET_4_0_PLUS
        /// <summary>
        /// Attempts to send the request. This process is asynchronous; unlike SendFluent, this method is awaitable and results will be delivered both to the caller and through events.
        /// <para>Upon success, the request is marked as sent.</para>
        /// <para>Whether this fails or not, the request will be disposed of.</para>
        /// </summary>
        /// <returns>The current request object.</returns>
        public /*async*/ Task<byte[]> SendAsync()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot send a disposed request!");

            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();

            AddResponseReceivedHandler(delegate(Request sender, BaseClient client, ResponseReceivedEventArgs e)
            {
                tcs.TrySetResult(e.Payload);
            }).AddTimeoutHandler(delegate(Request sender, BaseClient client, EventArgs e)
            {
                tcs.TrySetCanceled();
            }).AddFailureHandler(delegate(Request sender, BaseClient client, RequestFailureEventArgs e)
            {
                tcs.TrySetException(e.Exception);
            }).SendFluent();

            return /*await*/ tcs.Task;

            //return Task<byte[]>.Factory.FromAsync()
        }
#endif

        /// <summary>
        /// Attempts to send the request. This process is synchronous.
        /// <para>Upon success, the request is marked as sent.</para>
        /// <para>Whether this fails or not, the request will be disposed of.</para>
        /// </summary>
        /// <returns>The body of the response.</returns>
        public byte[] SendSynchronous()
        {
#if NET_4_0_PLUS
            return SendAsync().Result;
#else
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName, "Cannot send a disposed request!");

            byte[] resD = null;
            Exception resE = null;
            int res = 0;

            AddResponseReceivedHandler(delegate(Request sender, BaseClient client, ResponseReceivedEventArgs e)
            {
                resD = e.Payload;
                res = 1;
            }).AddTimeoutHandler(delegate(Request sender, BaseClient client, EventArgs e)
            {
                res = -1;
            }).AddFailureHandler(delegate(Request sender, BaseClient client, RequestFailureEventArgs e)
            {
                resE = e.Exception;
                res = 2;
            }).SendFluent();

            while (res == 0)
            {
#if NETFX_CORE
                Task.Delay(1).Wait();
#else
                Thread.Sleep(1);
#endif
            }

            switch (res)
            {
                case -1:
                case 1:
                    return resD;

                case 2:
                    throw resE;
            }

            return null;
#endif
        }
    }
}
