using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetwProg
{
    class Program
    {
        public static int port;
        static Thread ioThread;
        static Server server;
        static void Main(string[] args)
        {
            ReadInput(args);

            ioThread = new Thread(() => CreateInputThread());
            ioThread.Start();
        }

        static void ReadInput(string[] args)
        {
            string[] input = args;
            Console.Title = "NetChange " + input[0];
            port = int.Parse(input[0]);
            Data.AddNDisEntry(port, 0, port);
            server = new Server(port);
            for (int i = 1; i < input.Length; i++)
            {
                int nbport = int.Parse(input[i]);
                while (!Data.connections.ContainsKey(nbport))
                {
                    try
                    {
                        Connection newConnection = new Connection(nbport);
                    }
                    catch
                    {
                        Console.WriteLine("Trying to connect to " + nbport);
                        Thread.Sleep(50);
                    }
                }
                //Connection newConnection = new Connection(nbport);
                Data.AddNDisEntry(nbport, 1, nbport);
                //Data.ndis[nbport].AddPath(nbport, 1);
            }
        }
        static void CreateInputThread()
        {
            while (true)
            {
                Connection.ParseInput(Console.ReadLine());
            }
        }
    }
}
