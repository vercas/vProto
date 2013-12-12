using System;

namespace vProto
{
    using Events;

    /// <summary>
    /// Listens and handles connections.
    /// </summary>
    public abstract partial class BaseServer
        : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the object is disposed or not.
        /// </summary>
        public Boolean Disposed { get; private set; }

        /// <summary>
        /// Releases all the resources used by the current instance of vProto.Server.
        /// </summary>
        public virtual void Dispose()
        {
            for (int i = 0; i < _chs.Length; i++)
                if (_chs[i] != null && !_chs[i].Disposed)
                    _chs[i].Dispose();

            Disposed = true;
            IsOn = false;
        }



        /// <summary>
        /// Gets a value indicating whether the server is on and listening.
        /// </summary>
        public Boolean IsOn { get; protected set; }

        protected void _CheckIfStopped(Exception x)
        {
            if (IsOn)
            {
                Dispose();

                OnServerStopped(new ServerStoppedEventArgs(x));
            }
        }
    }
}
