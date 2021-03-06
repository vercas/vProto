﻿using System;

namespace vProto.Events
{
    /// <summary>
    /// Represents a method that will handle a client-related event with no data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An EventArgs that contains no event data.</param>
    public delegate void ClientEventHandler(BaseClient sender, EventArgs e);

    /// <summary>
    /// Represents a method that will handle a client-related event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event data generated by the event.</typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    public delegate void ClientEventHandler<TEventArgs>(BaseClient sender, TEventArgs e)
    where TEventArgs : EventArgs;


    /// <summary>
    /// Represents a method that will handle a request-related event with no data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="client">The client which the request is related to.</param>
    /// <param name="e">An System.EventArgs that contains no event data.</param>
    public delegate void RequestEventHandler(Request sender, BaseClient client, EventArgs e);

    /// <summary>
    /// Represents a method that will handle a request-related event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event data generated by the event.</typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="client">The client which the request is related to.</param>
    /// <param name="e">An System.EventArgs that contains the event data.</param>
    public delegate void RequestEventHandler<TEventArgs>(Request sender, BaseClient client, TEventArgs e)
    where TEventArgs : EventArgs;


    /// <summary>
    /// Represents a method that will handle a server-related event with no data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An System.EventArgs that contains no event data.</param>
    public delegate void ServerEventHandler(BaseServer sender, EventArgs e);

    /// <summary>
    /// Represents a method that will handle a server-related event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event data generated by the event.</typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An System.EventArgs that contains the event data.</param>
    public delegate void ServerEventHandler<TEventArgs>(BaseServer sender, TEventArgs e)
    where TEventArgs : EventArgs;


    //public delegate void ServerClientEventHandler(Server sender, BaseClient client, EventArgs e);

    //public delegate void ServerClientEventHandler<TEventArgs>(Server sender, BaseClient client, EventArgs e)
    //where TEventArgs : EventArgs;
}
