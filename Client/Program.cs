using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static vProto.BaseClient client;

        static void Main(string[] args)
        {
            if (!System.IO.File.Exists("ip.txt"))
            {
                Console.WriteLine("Please put the server's IP in a file named 'ip.txt' in the executable's folder.");
                Console.ReadLine();
                return;
            }



            string name = null;

            if (System.IO.File.Exists("name.txt"))
                name = System.IO.File.ReadAllText("name.txt");

            var ep = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(System.IO.File.ReadAllText("ip.txt")), 5665);

            if (name == null)
                client = new vProto.Protocols.TCP.Client(ep);
            else
                client = new vProto.Protocols.TCP.SSL.Client(ep, name);

            client.Disconnected += client_Disconnected;
            client.AuthFailed += client_AuthFailed;
            client.ConnectionFailed += client_ConnectionFailed;

            client.SendFailed += client_SendFailed;
            client.ReceiptFailed += client_ReceiptFailed;

            client.Connect();

            uint n;
            int a;

        go:
            string s = Console.ReadLine();

            if (s == "!")
            {
                client.Dispose();

                return;
            }
            else if (s == "hb")
            {
                if (!client.SendHeartbeat())
                    Console.WriteLine("Refused to send! {0} {1} {2}", client.IsConnected, client.IsAwaitingHeartbeat, client.IsSendingPacket);
            }
            else if (uint.TryParse(s, out n))
            {
                for (uint i = 0; i < n; i++)
                    client.SendData(UTF8Encoding.UTF8.GetBytes("" + (i + 1) + "/" + n + " - aswgergdsfwg34gr34g3tgwg35gh54gh34fgrgt34t34grdfsg"), (int)i);
            }
            else if (int.TryParse(s, out a))
            {
                a = -a;

                StringBuilder b = new StringBuilder(a + 1);

                for (int i = 0; i < a; i++)
                    b.Append('@');

                s = b.ToString();

                //client.SendShizzle("", 0);
                //client.SendShizzle(s, 9999);

                //client.CreateRequest(-1, Encoding.UTF8.GetBytes(s)).SetTimeout(1000).AddResponseReceivedHandler(responseHandler).SendFluent();
                try
                {
                    Console.WriteLine("Inlined request response: {0}", Encoding.UTF8.GetString(client.CreateRequest(-1, Encoding.UTF8.GetBytes(s)).SetTimeout(1000).SendAsync().Result));
                }
                catch (Exception x)
                {
                    Console.WriteLine("Inline request exception: {0}", x.ToString());
                }
            }
            else
            {
                //client.SendShizzle(s, 1337);

                client.CreateRequest(-1, Encoding.UTF8.GetBytes(s)).SetTimeout(1000).AddResponseReceivedHandler(responseHandler).SendFluent();
            }

            goto go;
        }

        static void responseHandler(vProto.Request sender, vProto.BaseClient client, vProto.Events.ResponseReceivedEventArgs e)
        {
            Console.WriteLine("Response received: {0}", Encoding.UTF8.GetString(e.Payload));
        }

        static void client_ReceiptFailed(vProto.BaseClient sender, vProto.Events.PipeFailureEventArgs e)
        {
            Console.WriteLine("Receipt failed!\n{0}", e.Exception);
        }

        static void client_SendFailed(vProto.BaseClient sender, vProto.Events.PipeFailureEventArgs e)
        {
            Console.WriteLine("Send failed!\n{0}", e.Exception);
        }

        static void client_ConnectionFailed(vProto.BaseClient sender, vProto.Events.ClientConnectionFailedEventArgs e)
        {
            Console.WriteLine("Connection failed..?\n{0}", e.Exception);
        }

        static void client_AuthFailed(vProto.BaseClient sender, vProto.Events.ClientAuthFailedEventArgs e)
        {
            Console.WriteLine("SSL Auth failed..?\n{0}", e.Exception);
        }

        static void client_Disconnected(vProto.BaseClient sender, vProto.Events.ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected!\n{0}", e.Exception);
        }
    }
}
