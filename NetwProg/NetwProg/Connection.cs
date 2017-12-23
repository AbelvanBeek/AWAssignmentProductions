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
                    //Print alles naar de console, behalve U(pdate) berichtjes
                    string input = Read.ReadLine();
                    if (! (input[0] == 'U'))
                        Console.WriteLine(input);
                    ParseInput(input);
                }
                
            }
            catch
            {
            } // Verbinding is kennelijk verbroken
        }

        public static void ParseInput(string inp)
        {
            //Neem een string als input en switch op wat voor berichtje het is (input[0])
            string[] input = inp.Split();
            string rest = "";
            for (int i = 2; i < input.Length; i++)
                rest += input[i] + " ";

            int newport;
            switch (input[0])
            {
                //print de routing tabel
                case "R":
                        Data.printRoutingTable();
                    break;
                //stuur een bericht
                case "B":
                    newport = int.Parse(input[1]);
                    int vianb = Data.ndis[newport].getShortestNdis().Key;
                    if (!Data.connections.ContainsKey(newport))
                    {
                        //stuur door naar dichstbijzijnde buur als de doelpoort niet in connections zit
                        try
                        {
                            Console.WriteLine("Bericht voor " + newport + " doorgestuurd naar " + vianb);
                            Data.connections[vianb].Write.WriteLine("B " + newport + " " + rest);
                        }
                        catch
                        {
                            //Er is blijkbaar geen neighbour via wie de goal bereikt kan worden
                        }

                    }
                    else
                    {
                        //in dit geval is er sprake van een direct berichtje
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
                //Maak connection
                case "C":
                    newport = int.Parse(input[1]);
                    if (!Data.connections.ContainsKey(newport))
                    {
                        Connection newConnection = new Connection(newport);
                        
                        lock (Data.computelock)
                        {
                            Data.AddNDisEntry(newport, 1, newport);
                        }
                    }
                    break;
                //Disconnect met een poort
                case "D":
                    lock (Data.computelock)
                    {
                        newport = int.Parse(input[1]);
                        try
                        {
                            //Stuur naar de poort waarmee we disconnecten een bericht dat hij ook met ons moet disconnecten
                            Data.connections[newport].Write.WriteLine("D " + Program.port);
                            lock (Data.dummy)
                            {
                                //verwijder de buur uit de connections lijst
                                Data.connections.Remove(newport);
                            }
                            //verwijder de poort uit de dis, zodat onze buren op de hoogte gesteld kunnen worden dat wij niet meer direct naar de verwijderde poort kunnen

                            lock (Data.dislock)
                            {
                                Data.dis.Remove(newport);
                            }

                            //Verwijder alle paden die via de disconnecte poort gaat
                            lock (Data.computelock)
                            {
                                Data.RemoveNeighbourFromNDis(newport);
                            }
                            Console.WriteLine("Verbroken: " + newport);
                        }
                        catch
                        {
                            Console.WriteLine("Poort " + newport + " is niet bekend");
                        }
                    }
                    break;
                //Update berichtje
                case "U":
                    newport = int.Parse(input[1]);
                    lock(Data.computelock)
                    {
                        for (int i = 2; i < input.Length; i += 2)
                        {
                            int to = int.Parse(input[i]);
                            //dist + 1, zodat onze buren de waarde van ons lezen en daar een bij optellen
                            int dist = int.Parse(input[i + 1]) + 1;

                            //Kijk of een node verwijderd moet worden uit de ndis, want de afstand kan nooit groter zijn dan N + 1
                            if (dist > Data.dis.Count)
                            {
                                //verwijder de betreffende node uit de ndis
                                Data.ndis.Remove(to);
                                //Stuur vervolgens eenmalig een berichtje naar onze neighbours dat wij die betreffende node niet meer kunnen bereiken
                                if (dist == (Data.dis.Count + 1))
                                {
                                    lock (Data.computelock)
                                    {
                                        foreach (KeyValuePair<int, Connection> nb in Data.connections)
                                        {
                                            try
                                            {
                                                nb.Value.Write.WriteLine("U " + Program.port + " " + to + " " + (Data.dis.Count + 1));
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                                Data.Recompute();
                            }
                            else
                            {
                                //Als de distance niet groter is dan de maximale lengte van een pad, behandel het U berichtje
                                Data.AddNDisEntry(to, dist, newport);
                                Data.Recompute();
                            }
                        }           
                    }

                    break;
                default:
                    //Als er een berichtje ontvangen wordt wat we niet kennen, doe niets
                    break;
            }
        }
    }
}
