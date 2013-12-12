using System;

namespace vProto.Events
{
    public delegate void ClientEventHandler(BaseClient sender, EventArgs e);

    public delegate void ClientEventHandler<TEventArgs>(BaseClient sender, TEventArgs e)
    where TEventArgs : EventArgs;


    public delegate void RequestEventHandler(Request sender, BaseClient client, EventArgs e);

    public delegate void RequestEventHandler<TEventArgs>(Request sender, BaseClient client, TEventArgs e)
    where TEventArgs : EventArgs;


    public delegate void ServerEventHandler(BaseServer sender, EventArgs e);

    public delegate void ServerEventHandler<TEventArgs>(BaseServer sender, TEventArgs e)
    where TEventArgs : EventArgs;


    //public delegate void ServerClientEventHandler(Server sender, BaseClient client, EventArgs e);

    //public delegate void ServerClientEventHandler<TEventArgs>(Server sender, BaseClient client, EventArgs e)
    //where TEventArgs : EventArgs;
}
