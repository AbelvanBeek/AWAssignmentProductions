using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetwProg
{
    public class Connection
    {
        public StreamReader Read;
        public StreamWriter Write;
        TcpClient client;
        int port;

        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port)
        {
            this.port = port;
            client = new TcpClient("localhost", port);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            Write.WriteLine("Poort: " + Program.port);

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
            Data.connections.Add(port, this);
            Console.WriteLine("Verbonden: " + port);
            Data.AddNDisEntry(port, 1);
            Console.WriteLine(port);
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write)
        {
            Read = read; Write = write;

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            try
            {
                while (true)
                {
                    string input = Read.ReadLine();
                    Console.WriteLine(input);
                    ParseInput(input);
                }
                //hier iets doen als shit veranderd
            }
            catch
            {
                Console.WriteLine("No Connection Found");
            } // Verbinding is kennelijk verbroken
        }
        public void Close()
        {
            Console.WriteLine("Verbroken: " + port);
            client.Close();
        }
        public static void ParseInput(string inp)
        {
            string[] input = inp.Split();
            string rest = "";
            for (int i = 2; i < input.Length; i++)
                rest += input[i] + " ";

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
                        connection.Write.WriteLine(rest);
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
