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
            ReadInput();
            server = new Server(port);
            ioThread = new Thread(() => CreateInputThread());
            ioThread.Start();
        }

        static void ReadInput()
        {
            string[] input = Console.ReadLine().Split();
            Console.Title = "NetChange " + input[0];
            port = int.Parse(input[0]);
            Data.AddNDisEntry(port);
            //Data.ndis[port].AddPath
            for (int i = 1; i < input.Length; i++)
            {
                int nbport = int.Parse(input[i]);
                //Connection newConnection = new Connection(nbport);
                Data.AddNDisEntry(nbport);
                Data.ndis[nbport].AddPath(nbport, 0);
                Data.ndis[port].AddPath(nbport, 1);
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
