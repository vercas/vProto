using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace vProto
{
    using Events;
    using Internals;
    using Packages;

    partial class BaseClient
    {
        internal List<int> _peers = null;
        private ReadOnlyCollection<int> _peers_ro = null;

        private List<int> _peers_queued_temp = new List<int>();

        internal object _peers_lock = new object();


        /// <summary>
        /// Gets a read only collection of IDs of peers.
        /// <para>A peer is a client connected to the same server.</para>
        /// <para>Returns null if peer discovery is disabled.</para>
        /// </summary>
        public ReadOnlyCollection<Int32> Peers
        {
            get
            {
                lock (_peers_lock)
                    return (_peers == null) ? null : ((_peers_ro == null) ? (_peers_ro = new ReadOnlyCollection<int>(_peers)) : _peers_ro);
            }
        }

        /// <summary>
        /// Gets whether peer discovery is enabled or not on the server associated with the current client.
        /// </summary>
        public Boolean PeerDiscoveryEnabled
        {
            get
            {
                return _peers != null;
            }
        }



        private void OnInternalPeerConnectedReceived(Package pack)
        {
            if (SERVER)
            {
                _CheckIfStopped(new InvalidOperationException("Server should not receive this package."), true);
            }
            else
            {
                lock (_peers_lock)
                    if (_peers == null)
                        _peers_queued_temp.Add(pack.Header.ID);
                    else if (!_peers.Contains(pack.Header.ID))
                    {
                        _peers.Add(pack.Header.ID);

                        OnPeerConnected(new PeerChangeEventArgs(pack.Header.ID, true));
                    }
            }
        }

        private void OnInternalPeerDisconnectedReceived(Package pack)
        {
            if (SERVER)
            {
                _CheckIfStopped(new InvalidOperationException("Server should not receive this package."), true);
            }
            else
            {
                lock (_peers_lock)
                    if (_peers == null)
                        _peers_queued_temp.Remove(pack.Header.ID);
                    else if (_peers.Remove(pack.Header.ID))
                        OnPeerDisconnected(new PeerChangeEventArgs(pack.Header.ID, false));
            }
        }



        internal void GivePeer(int id)
        {
            if (Disposed)
                return;

            bool send = false;

            lock (_peers_lock)
                if (_peers != null)
                    if (!_peers.Contains(id))
                    {
                        _peers.Add(id);

                        send = true;
                    }

            if (send)
            {
                try
                {
                    LowSendPackage(new PackageHeader { Type = PackageType.PeerConnected, ID = id }, emptyPayload);
                }
                catch { }   //  All exceptions need to be swallowed. The client may get disposed after the first if and before this one.

                OnPeerConnected(new PeerChangeEventArgs(id, true));
            }
        }

        internal void TakePeer(int id)
        {
            if (Disposed)
                return;

            bool send = false;

            lock (_peers_lock)
                if (_peers != null)
                    send = _peers.Remove(id);

            if (send)
            {
                try
                {
                    LowSendPackage(new PackageHeader { Type = PackageType.PeerDisconnected, ID = id }, emptyPayload);
                }
                catch { }   //  All exceptions need to be swallowed. The client may get disposed after the first if and before this one.

                OnPeerDisconnected(new PeerChangeEventArgs(id, false));
            }
        }
    }
}
