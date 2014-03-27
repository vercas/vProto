using System;

namespace vProto
{
    using Events;
    using Internals;

    /// <summary>
    /// Listens and handles connections.
    /// </summary>
    public abstract partial class BaseServer
        : _RequestHandler, IDisposable
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
            if (Disposed)
                return;

            for (int i = 0; i < _chs.Length; i++)
                if (_chs[i] != null && !_chs[i].Disposed)
                    _chs[i].Dispose();

            try
            {
                speedCountingTimer.Dispose();
            }
            catch (Exception)
            {

            }

            Disposed = true;
            IsOn = false;

            GC.SuppressFinalize(this);
        }



        /// <summary>
        /// Gets a value indicating whether the server is on and listening.
        /// </summary>
        public Boolean IsOn { get; protected set; }

        /// <summary>
        /// Halts the server and raises the appropriate events.
        /// </summary>
        /// <param name="x">Exception which caused halting.</param>
        protected void _CheckIfStopped(Exception x)
        {
            if (IsOn)
            {
                Dispose();

                OnStopped(new ServerStoppedEventArgs(x));
            }
        }
    }
}
