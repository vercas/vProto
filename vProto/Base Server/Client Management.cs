﻿using System;

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
                        //  I gotta subscribe to the event here (unfortunately, inside a lock) so it won't get added twice. This is the only code block guaranteed to run just once when this function is called.

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
                    _chs[h.ID] = null;
                // else what the heck is going on?
        }



        /* Events from clients...
         */



        void h_Disconnected(BaseClient sender, ClientDisconnectedEventArgs e)
        {
            _RemoveClient(sender);

            OnClientDisconnected(new ServerClientDisconnectedEventArgs(sender, e.Exception));
        }
    }
}
