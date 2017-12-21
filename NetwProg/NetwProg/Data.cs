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
         * ndis bevat op index 0: doelrouter, 1..*: buren (dus ook doelrouter) op oplopende index
         * 
         * dis bevat op index 0: doelrouter, 1: geschatte minimale distance uit ndis + 1
         */
        public static object dummy = new object();
        public static object computelock = new object();
        public static Dictionary<int,NDisEntry> ndis = new Dictionary<int, NDisEntry>();
        public static Dictionary<int, int> dis = new Dictionary<int, int>();
        public static Dictionary<int, Connection> connections = new Dictionary<int, Connection>();

        public static void AddDisEntry(int goal, int dist)
        {
                if (dis.ContainsKey(goal))
                    dis[goal] = dist;
                else dis.Add(goal, dist);
            //NOTIFY CODE HERE
        }
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
                //Recompute();
            }
        }
        public static void cleanNdisWithGivenDis(int nb, string s)
        {
            //
            //DIT WERKT NIET 
            //MOET OP EEN OF ANDERE MANIER ROUTES VERWIJDEREN DIE NIET MEER KUNNEN.

            //hier checken we of we een route gebruiken via buur nb die niet meer in de dis staat van buur nb,
            //die route moet dan namelijk verwijderd worden.
            string[] temp = s.Split();
            List<int> key = new List<int>();
            List<int> keys = ndis.Keys.ToList();
            List<int> kaas = new List<int>();
            //get all keys that aren't in the dis.
            for (int i = 0; i < temp.Count(); i += 2)
            {
                int x = int.Parse(temp[i]);
                    key.Add(x);
            }
            foreach(int x in keys)
            {
                if (!key.Contains(x))
                    kaas.Add(x);
            }
            //voor alle keys die niet in de dis zitten van de neighbour
            //verwijder het pad via die neighbour.
            for (int i = kaas.Count - 1; i > 0; i--)
            {
                ndis[kaas[i]].removePath(nb);
            }

        }
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
        }

        public static List<int> returnNeighbours()
        {
            //get list of neigbours from ndisentries
            if (ndis.Count > 0)
                return ndis[Program.port].returnAllNB();
            else
                return null;
        }

        public static bool ContainsNDis(int goal)
        {
            foreach (KeyValuePair<int,NDisEntry> entry in ndis)
            {
                if (entry.Key == goal)
                    return true;
            }
            return false;
        }

        public static bool contains(int port)
        {
            //check if we have a certain neigbour
            //check if the port is in the returnNeighbours.
            List<int> nb = returnNeighbours();
            if (nb == null)
                return false;
            return nb.Contains(port);
        }

        public static void Recompute()
        {
            foreach (KeyValuePair<int, NDisEntry> entry in ndis)
            {
                //all goals in ndis, check if its there, if so, check if it has changed 
                    //--> if so, change to smallest value and send message to neighbours, otherwise, skip

                int goal = entry.Key;
                int shortestDist = entry.Value.getShortestNdis().Value;
                int preferredNB = entry.Value.getShortestNdis().Key;

                if (goal == Program.port)
                {
                    if (dis.ContainsKey(goal))
                        continue;
                    dis.Add(goal, 0);
                }

                if (dis.ContainsKey(goal))
                {
                    if (shortestDist < dis[goal]) //if we have a shorter distance now.
                    {
                        dis[goal] = shortestDist;
                        Console.WriteLine("Afstand naar " + goal + " is nu " + (shortestDist) + " via " + preferredNB);
                        sendMessageToAllNeighbours(goal, (shortestDist));
                    }
                }
                else
                {
                    dis.Add(goal, shortestDist);
                    //send message to all neighbours
                    sendMessageToAllNeighbours(goal, (shortestDist));
                }
            }
        }

        public static void sendMessageToAllNeighbours(int goal, int dist)
        {
            lock (Data.dummy)
            {
                foreach (KeyValuePair<int, Connection> nb in Data.connections)
                {
                    try
                    {
                       nb.Value.Write.WriteLine("U " + Program.port + disToString());
                    }
                    catch
                    {
                        Console.WriteLine("Send message to neighbour: " + nb.Key + " failed, no direct connection with neighbour");
                    }
                }
            }

        }
        public static string disToString()
        {
            string s = "";
            foreach (KeyValuePair<int,int> entry in dis)
            {
                s += " " + entry.Key + " " + entry.Value;
            }
            return s;
        }

        public static void printRoutingTable()
        {
            
            foreach (KeyValuePair<int, NDisEntry> entry in ndis)
            {
                entry.Value.print();
            }
        }
    }

}
