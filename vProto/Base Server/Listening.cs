using System;
using System.Threading;

namespace vProto
{
    using Events;

    partial class BaseServer
    {
        /// <summary>
        /// Starts listening to incomming connection requests asynchronously.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (IsOn)
                return false;

            try
            {
                speedCountingTimer = new Timer(__speedCountingTimerCallback, null, new TimeSpan(0), new TimeSpan(0, 0, 1));

                return StartListening();
            }
            catch (Exception x)
            {
                OnStartupFailed(new ServerStartupFailedEventArgs(x));

                return false;
            }
        }
    }
}
