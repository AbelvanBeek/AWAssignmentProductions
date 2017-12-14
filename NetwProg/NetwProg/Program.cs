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
        static Thread ioThread, connectionhandler;
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
            Data.dis.Add(new int[2] { int.Parse(input[0]), 0});
            for (int i = 1; i < input.Length; i++)
            {
                int nbport = int.Parse(input[i]);
                //Connection connection = new Connection(nbport);
                Data.AddDisEntry(nbport, 1);
            }
        }
        static void CreateInputThread()
        {
            while (true)
            {

                string[] input = Console.ReadLine().Split();
                int newport;
                switch (input[0])
                {
                    case "R":
                        Data.printRoutingTable();
                        break;
                    case "B":
                        //Stuur bericht
                        newport = int.Parse(input[1]);
                        if (!Data.returnNeighbours().Contains(newport))
                        {
                            //stuur door naar dichstbijzijnde buur.
                        }
                        try
                        {
                            Connection connection = Data.connections[newport];
                            connection.Write.WriteLine(input[2]);
                        }
                        catch
                        {
                            Console.WriteLine("Poort " + newport + " is niet bekend");
                        }
                        break;
                    case "C":
                        //maak connection
                        newport = int.Parse(input[1]);
                        if (!Data.contains(newport))
                        {
                            Connection newConnection = new Connection(newport);
                            newConnection.Write.WriteLine("C " + port);
                        }
                        break;
                    case "D":
                        newport = int.Parse(input[1]);
                        try
                        {
                            Data.connections[newport].Close();
                            Data.connections.Remove(newport);
                        }
                        catch
                        {
                            Console.WriteLine("Poort " + newport + " is niet bekend");
                        }
                        break;
                    default:
                        //
                        break;
                }
            }
        }
    }
}
