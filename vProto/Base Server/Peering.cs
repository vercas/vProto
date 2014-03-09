using System;
using System.Collections.Generic;

namespace vProto
{
    using Events;

    partial class BaseServer
    {
        internal bool _peering = true;

        /// <summary>
        /// Gets or sets whether clients are given peer IDs or not.
        /// <para>May only be set before starting the server.</para>
        /// </summary>
        public bool PeerDiscoveryEnabled
        {
            get
            {
                return _peering;
            }
            set
            {
                if (IsOn)
                    throw new InvalidOperationException("Must set before starting server.");

                _peering = value;
            }
        }



        /// <summary>
        /// Builds a list of IDs of peers of a given client.
        /// <para>Will return null if peer discovery is disabled.</para>
        /// </summary>
        /// <param name="id">The ID of the client whose peers to retrieve.</param>
        /// <returns>List of IDs of peers if peer discovery is enabled; otherwise false.</returns>
        internal protected List<int> GetClientPeersID(int id)
        {
            if (!_peering)
                return null;

            var res = new List<int>();

            lock (_chs_sync)
                for (int i = 0; i < _chs.Length; i++)
                    if (i != id && _chs[i] != null)
                        res.Add(i);

            return res;
        }
    }
}
