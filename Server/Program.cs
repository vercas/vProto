﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static vProto.BaseServer serv;

        static void Main(string[] args)
        {
            System.Security.Cryptography.X509Certificates.X509Certificate2 cert = null;

            if (System.IO.File.Exists("cert.pfx") && System.IO.File.Exists("pw.txt"))
                cert = new System.Security.Cryptography.X509Certificates.X509Certificate2("cert.pfx", System.IO.File.ReadAllText("pw.txt"));

            //serv = new vProto.BaseServer(5665, cert);
            if (cert == null)
                serv = new vProto.Protocols.TCP.Server(5665);
            else
                serv = new vProto.Protocols.TCP.SSL.Server(5665, cert);

            serv.ClientConnected += serv_ClientConnected;

            serv.Start();

            var updater = new Timer(new TimerCallback(delegate(object state)
            {
                Console.Title = string.Format("Speed: In {0}; Out {1}", serv.IncommingSpeed, serv.OutgoingSpeed);

            }), null, 1000, 1000);

            Console.ReadLine();

            serv.Dispose();

            updater.Dispose();
        }

        static void serv_ClientConnected(vProto.BaseServer sender, vProto.Events.ServerClientConnectedEventArgs e)
        {
            Console.WriteLine("Client CONNECTED\t{0}", e.ID);

            e.Client.Disconnected += Client_Disconnected;
            e.Client.RequestReceived += Client_RequestReceived;
            e.Client.SendFailed += Client_SendFailed;
            e.Client.ReceiptFailed += Client_ReceiptFailed;

            e.Client.RegisterRmiService<Common_Test_Shizzle.RMI_Interface>(new Common_Test_Shizzle.RMI_Object());
        }

        static void Client_ReceiptFailed(vProto.BaseClient sender, vProto.Events.PipeFailureEventArgs e)
        {
            Console.WriteLine("Receipt failed for {0}", sender.ID);
        }

        static void Client_SendFailed(vProto.BaseClient sender, vProto.Events.PipeFailureEventArgs e)
        {
            Console.WriteLine("Send failed for {0}", sender.ID);
        }

        static void Client_RequestReceived(vProto.BaseClient sender, vProto.Events.RequestReceivedEventArgs e)
        {
            Console.WriteLine("Received request of {0} byes from {1}.", e.Response.RequestPayload.Length, sender.ID);

            e.Response.SetPayload(Encoding.UTF8.GetBytes(e.Response.RequestPayload.Length.ToString())).Send();
        }

        static void Client_Disconnected(vProto.BaseClient sender, vProto.Events.ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Client {0} DISCONNECTED", sender.ID);
        }
    }
}
