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
        static object kaas = new object();
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
            //Try to make a connections with all the neighbours, if not possible, try again
            for (int i = 1; i < input.Length; i++)
            {
                int nbport = int.Parse(input[i]);
                if (nbport > port)
                {
                    while (!Data.connections.ContainsKey(nbport))
                    {

                        try
                        {
                            Connection newConnection = new Connection(nbport);
                        }
                        catch
                        {
                            Console.WriteLine("Trying to connect to " + nbport);
                            Thread.Sleep(0);
                        }
                    }
                }
            }
            //Thread.Sleep(5000);
            foreach (KeyValuePair<int, Connection> nb in Data.connections)
            {
                Data.AddNDisEntry(nb.Key, 1, nb.Key);
            }
            Data.sendMessageToAllNeighbours();
        }

        //Code for the thread that reads from the console
        static void CreateInputThread()
        {
            while (true)
            {
                Connection.ParseInput(Console.ReadLine());
            }
        }
    }
}
