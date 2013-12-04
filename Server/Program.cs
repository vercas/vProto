using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Console.ReadLine();

            serv.Dispose();
        }

        static void serv_ClientConnected(vProto.BaseServer sender, vProto.Events.ServerClientConnectedEventArgs e)
        {
            Console.WriteLine("Client CONNECTED\t{0}", e.ID);

            e.Client.Disconnected += Client_Disconnected;
            e.Client.RequestReceived += Client_RequestReceived;
            e.Client.SendFailed += Client_SendFailed;
            e.Client.ReceiptFailed += Client_ReceiptFailed;
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
            Console.WriteLine("Received request \"{0}\" from {1}.", Encoding.UTF8.GetString(e.Payload), sender.ID);
        }

        static void Client_Disconnected(vProto.BaseClient sender, vProto.Events.ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Client {0} DISCONNECTED", sender.ID);
        }
    }
}
