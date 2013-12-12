using System;

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
                return StartListening();
            }
            catch (Exception x)
            {
                OnServerStartupFailed(new ServerStartupFailedEventArgs(x));

                return false;
            }
        }
    }
}
