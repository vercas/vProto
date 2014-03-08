using System;
using System.Collections.Generic;

namespace vProto
{
    using Events;

    partial class BaseServer
    {
        protected void ClientReceived(BaseClient client)
        {
            _AddClient(client);

            OnClientConnected(new ServerClientConnectedEventArgs(client));
        }



        internal BaseClient[] _chs = new BaseClient[10];
        object _chs_sync = new object();



        private void _DoubleClientContainer()
        {
            lock (_chs_sync)
            {
                BaseClient[] n = new BaseClient[_chs.Length * 2];

                _chs.CopyTo(n, 0);
                //  Indices need to be preserved...

                _chs = n;
            }
        }

        private int _AddClient(BaseClient h)
        {
            lock (_chs_sync)
                for (int i = 0; i < _chs.Length; i++)   //  Even with 100,000 clients, this loop won't be an issue.
                    if (_chs[i] == null)
                    {
                        h.Disconnected += h_Disconnected;
                        h.ConnectionFailed += h_ConnectionFailed;
                        //  I gotta subscribe to the event here (unfortunately, inside a lock) so it won't get added twice. This is the only code block guaranteed to run just once when this function is called.

                        if (_peering)
                            for (int j = 0; j < _chs.Length; j++)
                                if (_chs[j] != null)
                                    _chs[j].GivePeer(i);

                        return (_chs[i] = h)._id = i;
                        //  I love assignment as expression!
                    }

            //  If a suitable position wasn't found...

            _DoubleClientContainer();

            return _AddClient(h);
        }

        private void _RemoveClient(BaseClient h)
        {
            lock (_chs_sync)
                if (h.ID > -1 && h.ID < _chs.Length)
                {
                    _chs[h.ID] = null;

                    if (_peering)
                        for (int j = 0; j < _chs.Length; j++)
                            if (_chs[j] != null)
                                _chs[j].TakePeer(h.ID);
                }
                // else what the heck is going on?
        }



        /* Events from clients...
         */



        void h_Disconnected(BaseClient sender, ClientDisconnectedEventArgs e)
        {
            _RemoveClient(sender);

            OnClientDisconnected(new ServerClientDisconnectedEventArgs(sender, e.Exception));
        }

        void h_ConnectionFailed(BaseClient sender, ClientConnectionFailedEventArgs e)
        {
            _RemoveClient(sender);

            OnClientConnectionFailed(new ServerClientConnectionFailedEventArgs(e.Exception));
        }



        /* Utilitary methods...
         */



        /// <summary>
        /// Builds a list of IDs of connected clients.
        /// </summary>
        /// <returns></returns>
        public List<int> ListAllClientIDs()
        {
            var res = new List<int>();

            lock (_chs_sync)
                for (int i = 0; i < _chs.Length; i++)
                    if (_chs[i] != null)
                        res.Add(i);

            return res;
        }



        /// <summary>
        /// Gets the vProto.BaseClient object with the specified ID.
        /// </summary>
        /// <param name="index">The client ID.</param>
        /// <returns></returns>
        public BaseClient this[int id]
        {
            get { return (id >= 0 && id < _chs.Length) ? _chs[id] : null; }
        }
    }
}
