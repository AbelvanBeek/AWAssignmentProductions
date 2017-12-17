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

        public static List<NDisEntry> ndis = new List<NDisEntry>();
        public static Dictionary<int, int> dis = new Dictionary<int, int>();
        public static Dictionary<int, Connection> connections = new Dictionary<int, Connection>();

        public static void AddDisEntry(int goal, int dist)
        {
            if (dis.ContainsKey(goal))
                dis[goal] = dist;
            else dis.Add(goal, dist);
            //NOTIFY CODE HERE
        }
        public static void AddNDisEntry(int goal)
        {
            if (!ContainsNDis(goal))
            {
                NDisEntry entry = new NDisEntry(goal);
                ndis.Add(entry);
                Recompute();
            }
            else
            {
                //ndis[goal].AddPath()
                Console.WriteLine("Adding to NDIS failed: already in NDIS");
            }
        }

        public static List<int> returnNeighbours()
        {
            //get list of neigbours from ndisentries
            if (ndis.Count > 0)
                return ndis[0].returnAllNB();
            else
                return null;
        }

        public static bool ContainsNDis(int goal)
        {
            foreach (NDisEntry entry in ndis)
            {
                if (entry.goal == goal)
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
            foreach (NDisEntry entry in ndis)
            {
                //all goals in ndis, check if its there, if so, check if it has changed 
                    //--> if so, change to smallest value and send message to neighbours, otherwise, skip

                int goal = entry.goal;
                int shortestDist = entry.getShortestNdis().Value;

                if (dis.ContainsKey(goal))
                {
                    if (dis[goal] != shortestDist)
                    {
                        dis[goal] = shortestDist;
                        //send message to all neighbours that value for goal has changed
                        sendMessageToAllNeighbours(goal, shortestDist);
                    }
                }
                else
                {
                    dis.Add(goal, shortestDist);
                    //send message to all neighbours
                    sendMessageToAllNeighbours(goal, shortestDist);
                }
            }
        }

        public static void sendMessageToAllNeighbours(int goal, int dist)
        {
            List<int> nb = returnNeighbours();
            if (nb != null)
            {
                foreach (int neigb in nb)
                {
                    try
                    {
                        Connection connection = Data.connections[neigb];
                        connection.Write.WriteLine("U " + goal + " " + dist);
                    }
                    catch
                    {
                        Console.WriteLine("Deze man heeft nog geen connection: " + neigb);
                    }
                }
            }
        }

        public static void printRoutingTable()
        {
            
            Console.WriteLine(Program.port + " " + 0 + " local");
            foreach (NDisEntry entry in ndis)
            {
                entry.print();
            }
        }
    }

}
