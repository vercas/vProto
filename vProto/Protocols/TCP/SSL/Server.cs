﻿using System;
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

namespace vProto.Protocols.TCP.SSL
{
    using Events;

    /// <summary>
    /// A TCP/IP server.
    /// </summary>
    public sealed class Server
        : vProto.Protocols.TCP.Server
    {
        X509Certificate cert;



        /// <summary>
        /// Initializes a new instance of the vProto.Protocols.TCP.SSL.Server which will listen for connections on the specified port.
        /// </summary>
        /// <param name="port">The port on which to listen for connections.</param>
        /// <param name="certificate">The certificate used to authenticate the server.</param>
        public Server(int port, X509Certificate certificate)
            : base(port)
        {
            cert = certificate;
        }



        protected override void HandleTcpClient(TcpClient client)
        {
            ClientReceived(new Client(client, cert));
        }

        //  Unlike the parent's handling function, this one feeds in a new type of client to the base server.
    }
}