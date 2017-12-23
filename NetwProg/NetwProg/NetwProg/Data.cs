using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetwProg
{
    public static class Data
    {
        /*
         * ndis bevat de doelrouter in de key, de value is een ndisentry. Voor meer informatie, zie NDisEntry.cs.
         * 
         * dis bevat de doelrouter in de key, de value bevat een schatting van de kortste afstand naar die doelrouter.
         */
        public static object dummy = new object();
        public static object computelock = new object();
        public static object dislock = new object();

        public static Dictionary<int,NDisEntry> ndis = new Dictionary<int, NDisEntry>();
        public static Dictionary<int, int> dis = new Dictionary<int, int>();
        public static Dictionary<int, Connection> connections = new Dictionary<int, Connection>();

        //Voeg een ndisentry toe aan de ndis. Als deze goal al aanwezig is, passen we de kosten aan in de disViaNB dictionary in de NDisEntry bij de juiste 'via neighbour'
        public static void AddNDisEntry(int goal, int dist, int viaport)
        {
            lock (Data.computelock)
            {
                if (!ContainsNDis(goal))
                {
                    NDisEntry entry = new NDisEntry(goal);
                    entry.AddPath(viaport, dist);
                    ndis.Add(goal, entry);
                }
                else
                {
                    ndis[goal].AddPath(viaport, dist);
                }
                Recompute();
            }
        }

        //Verwijderd alle paden die via nbPort lopen uit de ndis
        public static void RemoveNeighbourFromNDis(int nbPort)
        {
            List<int> keys = ndis.Keys.ToList();
            for (int i = keys.Count -1; i > 0; i--)
            {
                ndis[keys[i]].removePath(nbPort);
                if(ndis[keys[i]].length() == 0)
                {
                    ndis.Remove(keys[i]);
                }
            }
            Recompute();
        }
        
        //Geeft een lijst terug van alle neighbours, gebaseerd op de ndis
        public static List<int> returnNeighbours()
        {
            //get list of neigbours from ndisentries
            if (ndis.Count > 0)
                return ndis[Program.port].returnAllNB();
            else
                return null;
        }
        
        //Kijkt op een goal bevat is in de ndis
        public static bool ContainsNDis(int goal)
        {

            foreach (KeyValuePair<int,NDisEntry> entry in ndis)
            {
                if (entry.Key == goal)
                    return true;
            }
            return false;
        }
        
        //De befaamde recompute methode
        public static void Recompute()
        {
            //loop over de hele ndis
            foreach (KeyValuePair<int, NDisEntry> entry in ndis)
            {

                int goal = entry.Key;
                int shortestDist = ndis[goal].getShortestNdis().Value;// entry.Value.getShortestNdis().Value;
                int preferredNB = entry.Value.getShortestNdis().Key;
                
                //Als de goal onszelf is, voeg onszelf toe aan de dis met distance 0
                if (goal == Program.port)
                {
                    if (dis.ContainsKey(goal))
                        continue;
                    lock (dislock)
                    {
                        dis.Add(goal, 0);
                    }
                }

                //als de goal al in de dis lijst zit
                if (dis.ContainsKey(goal))
                {
                    //kijk of de afstand tot die goal veranderd is
                    if (shortestDist != dis[goal]) 
                    {
                        //zo ja, pas de distance in onze dis aan, laat een melding zien dat de afstand is aangepast en wellicht het pad nu via een andere neighbour loopt
                        dis[goal] = shortestDist;
                        Console.WriteLine("Afstand naar " + goal + " is nu " + (shortestDist) + " via " + preferredNB);
                        //stuur vervolgens onze nieuwe dis naar onze neighbours
                        sendMessageToAllNeighbours();
                        Console.WriteLine(shortestDist);
                    }

                }
                else
                {
                    //goal niet in dis? Dan voegen we hem toe en sturen we wederom onze dis naar onze neighbours
                    lock (dislock)
                    {
                        dis.Add(goal, shortestDist);
                    }
                    sendMessageToAllNeighbours();
                }
            }
        }

        public static void sendMessageToAllNeighbours()
        {
            //zorg dat de connection lijst niet wordt aangepast terwijl we deze berichten sturen
            lock (Data.dummy)
            {
                foreach (KeyValuePair<int, Connection> nb in Data.connections)
                {
                    try
                    {
                        //stuur de gehele dis naar onze neighbours
                        nb.Value.Write.WriteLine("U " + Program.port + disToString());
                    }
                    catch
                    {
                        Console.WriteLine("Send message to neighbour: " + nb.Key + " failed, no direct connection with neighbour");
                    }
                }
            }

        }

        //Converteert de dis naar een string die meegestuurt kan worden in een update berichtje
        public static string disToString()
        {
            string s = "";
            foreach (KeyValuePair<int,int> entry in dis)
            {
                s += " " + entry.Key + " " + entry.Value;
            }
            return s;
        }

        //voor elk item in de ndis, print deze naar het scherm
        public static void printRoutingTable()
        {
            lock (computelock)
            {
                foreach (KeyValuePair<int, NDisEntry> entry in ndis)
                {
                    entry.Value.print();
                }
            }

        }
    }

}
