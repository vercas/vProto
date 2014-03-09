using System;

namespace vProto.Events
{
    /// <summary>
    /// Provides data for the vProto.BaseClient.PeerConnected and vProto.BaseClient.PeerDisconnected events. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class PeerChangeEventArgs
        : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the vProto.Events.PeerChangeEventArgs with the specified response object.
        /// </summary>
        /// <param name="id">The ID of the (dis)connected peer.</param>
        /// <param name="connected">True if client connected; otherwise False (if disconnected).</param>
        public PeerChangeEventArgs(int id, bool connected)
            : base()
        {
            ID = id;
            _con = connected;
        }


        /// <summary>
        /// Gets the ID of the peer who (dis)connected.
        /// </summary>
        public Int32 ID { get; private set; }

        private bool _con = false;

        /// <summary>
        /// Gets whether the peer has connected (true) or disconnected (false).
        /// </summary>
        public Boolean Connected { get { return _con; } }

        /// <summary>
        /// Gets whether the peer has disconnected (true) or connected (false).
        /// </summary>
        public Boolean Disconnected { get { return !_con; } }
    }
}
