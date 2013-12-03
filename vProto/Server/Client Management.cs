using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace vProto
{
    using vProto.Events;

    partial class Server
    {
        internal ClientHandler[] _chs = new ClientHandler[10];
        object _chs_sync = new object();



        private void _DoubleClientContainer()
        {
            lock (_chs_sync)
            {
                ClientHandler[] n = new ClientHandler[_chs.Length * 2];

                _chs.CopyTo(n, 0);
                //  Indices need to be preserved...

                _chs = n;
            }
        }

        private int _AddClient(ClientHandler h)
        {
            lock (_chs_sync)
                for (int i = 0; i < _chs.Length; i++)   //  Even with 100,000 clients, this loop won't be an issue.
                    if (_chs[i] == null)
                    {
                        h.Disconnected += h_Disconnected;
                        //  I gotta subscribe to the event here (unfortunately, inside a lock) so it won't get added twice. This is the only code block guaranteed to run just once when this function is called.

                        return (_chs[i] = h).ID = i;
                        //  I love assignment as expression!
                    }

            //  If a suitable position wasn't found...

            _DoubleClientContainer();

            return _AddClient(h);
        }

        private void _RemoveClient(ClientHandler h)
        {
            lock (_chs_sync)
                if (h.ID > -1 && h.ID < _chs.Length)
                    _chs[h.ID] = null;
                // else what the heck is going on?
        }



        /* Events from clients...
         */



        void h_Disconnected(BaseClient sender, Events.ClientDisconnectedEventArgs e)
        {
            var handler = sender as ClientHandler;

            _RemoveClient(handler);

            OnClientDisconnected(new ServerClientDisconnectedEventArgs(handler, e.Exception));
        }
    }
}
