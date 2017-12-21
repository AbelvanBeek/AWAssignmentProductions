﻿using System;
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
        object write = new object();

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
            lock (Data.dummy)
            {
                Data.connections.Add(port, this);
            }
            Console.WriteLine("Verbonden: " + port);
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write)
        {
            Read = read; Write = write;
            //Console.WriteLine("Verbonden: " + port);
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
                    //if (! (input[0] == 'U'))
                    Console.WriteLine(input);
                    ParseInput(input);
                }
                //hier iets doen als shit veranderd
            }
            catch
            {
                //Console.WriteLine("No Connection Found");
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
                    int vianb = Data.ndis[newport].getShortestNdis().Key;
                    if (!Data.connections.ContainsKey(newport))
                    {
                        //stuur door naar dichstbijzijnde buur.
                        try
                        {
                            Console.WriteLine("Bericht voor " + newport + " doorgestuurd naar " + vianb);
                            Data.connections[vianb].Write.WriteLine("B " + newport + " " + rest);
                        }
                        catch(Exception e)
                        {
                            //Console.WriteLine(e);
                            //Console.WriteLine("Tried to forward message " + rest + " to " + newport + " but failed");
                        }

                    }
                    else
                    {
                        try
                        {
                            Connection connection = Data.connections[newport];
                            Console.WriteLine("Bericht voor " + newport + " doorgestuurd naar " + vianb);
                            connection.Write.WriteLine(rest);

                        }
                        catch
                        {
                            Console.WriteLine("Poort " + newport + " is niet bekend");
                        }
                    }

                    break;
                case "C":
                    //maak connection
                    newport = int.Parse(input[1]);
                    if (!Data.connections.ContainsKey(newport))
                    {
                        Connection newConnection = new Connection(newport);
                        //DIT HIERONDER KAN TEMP ZIJN NU WE NOG MET C EEN CONNECTION MAKEN
                        Data.AddNDisEntry(newport, 1, newport);
                        //Data.ndis[newport].AddPath(newport, 1);
                    }
                    break;
                case "D":
                    newport = int.Parse(input[1]);
                    try
                    {
                        Data.connections.Remove(newport);
                        //Data.connections[newport].Close();
                        Data.RemoveNeighbourFromNDis(newport);
                        //remove from Ndis
                        Console.WriteLine("Verbroken: " + newport);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("Poort " + newport + " is niet bekend");
                    }
                    break;
                case "U":
                    //Eerst ndis updaten en dan recompute
                    newport = int.Parse(input[1]); //newport = goal
                    
                    for (int i = 2; i < input.Length; i += 2)
                    {
                        Data.AddNDisEntry(int.Parse(input[i]), int.Parse(input[i + 1]) + 1, newport);
                        //we moeten voor iedere plek waar we de zender van dit bericht als pad hebben kijken of hij nog steeds een pad heeft.
                    }
                    //Data.cleanNdisWithGivenDis(newport, rest);
                    //Data.AddNDisEntry(newport, int.Parse(input[2]) + 1, int.Parse(input[3])); //input[3] is via welke neighbour | input[2] is de distance
                    break;
                default:
                    //
                    break;
            }
        }
    }
}
