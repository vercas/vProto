using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Client
{
    class Program
    {
        static vProto.BaseClient client;
        static List<vProto.BaseClient> others = new List<vProto.BaseClient>();

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

            //ThreadPool.SetMaxThreads(40, 40);

            var updater = new Timer(new TimerCallback(delegate(object state)
            {
                var peers = client.Peers;
                string peerStr = string.Empty;

                if (peers == null)
                {
                    peerStr = "No peering!";
                }
                else
                {
                    peerStr = "#" + peers.Count;

                    if (peers.Count < 15)
                        for (int i = 0; i < peers.Count; i++)
                            if (i == 0)
                                peerStr += ": " + peers[i];
                            else
                                peerStr += ", " + peers[i];
                }

                Console.Title = string.Format("Speed: In {0}; Out {1}; Ping: {2}; ID: {3}; Peers: {4}", client.IncommingSpeed, client.OutgoingSpeed, client.Ping.TotalMilliseconds, client.ID, peerStr);

            }), null, 1000, 1000);

            uint n;
            int a;

        go:
            string s = Console.ReadLine();

            if (s == "!")
            {
                if (!client.Disposed)
                    client.Dispose();

                foreach (var c in others)
                    if (!c.Disposed)
                        c.Dispose();

                updater.Dispose();

                return;
            }
            else if (s == "hb")
            {
                if (!client.SendHeartbeat())
#if SENDER_THREAD
                    Console.WriteLine("Refused to send! {0} {1}", client.IsConnected, client.IsAwaitingHeartbeat);
#else
                    Console.WriteLine("Refused to send! {0} {1} {2}", client.IsConnected, client.IsAwaitingHeartbeat, client.IsSendingPackage);
#endif
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
                    Console.WriteLine("Inlined request response: {0}", Encoding.UTF8.GetString(client.CreateRequest(-1, Encoding.UTF8.GetBytes(s)).SetTimeout(10000).SendSynchronous()));
                }
                catch (Exception x)
                {
                    Console.WriteLine("Inline request exception: {0}", x.ToString());
                }
            }
            else if (s == "fail")
            {
                try
                {
                    client.ProxyRmiService<Common_Test_Shizzle.RMI_Interface>().Fail();
                }
                catch(Exception x)
                {
                    Console.WriteLine("Got exception from RMI: {0}", x);
                }
            }
            else if (s.Length > 4 && s.Substring(0, 4) == "spn ")
            {
                var cnt = int.Parse(s.Substring(4));

                for (int i = 0; i < cnt; i++)
                {
                    vProto.BaseClient c;

                    if (name == null)
                        c = new vProto.Protocols.TCP.Client(ep);
                    else
                        c = new vProto.Protocols.TCP.SSL.Client(ep, name);

                    others.Add(c);

                    c.Connect();

                    Thread.Sleep(10);
                }
            }
            else if (s.Length > 4 && s.Substring(0, 4) == "out ")
            {
                try
                {
                    int asd = -1;

                    var res = client.ProxyRmiService<Common_Test_Shizzle.RMI_Interface>().Blah(s.Substring(4), out asd);

                    Console.WriteLine("Out parameter test result: {0}, {1}", res, asd);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed RMI out test...");
                    Console.WriteLine(x.ToString());
                }
            }
            else if (s == "propget")
            {
                try
                {
                    Console.WriteLine("Property is: {0}", client.ProxyRmiService<Common_Test_Shizzle.RMI_Interface>().Proparoo);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed RMI propget test...");
                    Console.WriteLine(x.ToString());
                }
            }
            else if (s.Length > 8 && s.Substring(0, 8) == "propset ")
            {
                try
                {
                    client.ProxyRmiService<Common_Test_Shizzle.RMI_Interface>().Proparoo = s.Substring(8);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed RMI propset test...");
                    Console.WriteLine(x.ToString());
                }
            }
            else
            {
                //client.SendShizzle(s, 1337);

                //client.CreateRequest(-1, Encoding.UTF8.GetBytes(s)).SetTimeout(1000).AddResponseReceivedHandler(responseHandler).SendFluent();

                try
                {
                    Console.WriteLine("Inline RMI call result: {0}", client.ProxyRmiService<Common_Test_Shizzle.RMI_Interface>().Test(s));
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed RMI call...");
                    Console.WriteLine(x.ToString());
                }
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
