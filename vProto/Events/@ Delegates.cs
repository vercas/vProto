using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto.Events
{
    public delegate void ClientEventHandler(BaseClient sender, EventArgs e);

    public delegate void ClientEventHandler<TEventArgs>(BaseClient sender, TEventArgs e)
    where TEventArgs : EventArgs;

    public delegate void RequestEventHandler(Request sender, BaseClient client, EventArgs e);

    public delegate void RequestEventHandler<TEventArgs>(Request sender, BaseClient client, TEventArgs e)
    where TEventArgs : EventArgs;
}
