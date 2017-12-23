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
        public static Dictionary<int,NDisEntry> ndis = new Dictionary<int, NDisEntry>();
        public static Dictionary<int, int> dis = new Dictionary<int, int>();
        public static Dictionary<int, Connection> connections = new Dictionary<int, Connection>();

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
        
        public static void Recompute()
        {
            foreach (KeyValuePair<int, NDisEntry> entry in ndis)
            {
                //all goals in ndis, check if its there, if so, check if it has changed 
                    //--> if so, change to smallest value and send message to neighbours, otherwise, skip

                int goal = entry.Key;
                int shortestDist = ndis[goal].getShortestNdis().Value;// entry.Value.getShortestNdis().Value;
                int preferredNB = entry.Value.getShortestNdis().Key;
                
                if (goal == Program.port)
                {
                    if (dis.ContainsKey(goal))
                        continue;
                    dis.Add(goal, 0);
                }

                if (dis.ContainsKey(goal))
                {
                    if (shortestDist != dis[goal]) //if we have a shorter or longer distance now.
                    {
                        dis[goal] = shortestDist;
                        Console.WriteLine("Afstand naar " + goal + " is nu " + (shortestDist) + " via " + preferredNB);
                        sendMessageToAllNeighbours();
                        Console.WriteLine(shortestDist);
                    }

                }
                else
                {
                    dis.Add(goal, shortestDist);
                    //send message to all neighbours
                    sendMessageToAllNeighbours();
                }
            }
            //Console.WriteLine(disToString());
        }

        public static void sendMessageToAllNeighbours()
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

        /*
        public static void deleteMessage(int port)
        {
            int newdis = -1;
            try
            {
                newdis = ndis[port].getShortestNdis().Value;
            }
            catch
            {

            }
            foreach (KeyValuePair<int, Connection> nb in Data.connections)
            {
                try
                {
                    nb.Value.Write.WriteLine("DEL " +  port + " " + Program.port + " " + newdis);
                }
                catch
                {
                    Console.WriteLine("Send message to neighbour: " + nb.Key + " failed, no direct connection with neighbour");
                }
            }
        }
        */
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
