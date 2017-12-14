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
        public static List<int[]> dis = new List<int[]>();
        public static Dictionary<int, Connection> connections = new Dictionary<int, Connection>();

        public static void AddDisEntry(int goal, int dist)
        {
            dis.Add(new int[2] { goal, dist });
            //NOTIFY CODE HERE
        }
        public static void AddNDisEntry(int goal, int dist)
        {
            NDisEntry entry = new NDisEntry(goal);
            //entry.AddPath()
            ndis.Add(entry);
            //NOTIFY CODE HERE
        }
        public static List<int> returnNeighbours()
        {
            //get list of neigbours form ndisentries
            if (ndis.Count > 0)
                return ndis[0].returnAllNB();
            else
                return null;
        }
        public static bool contains(int port)
        {
            //check if the port is in the returnNeighbours.
            List<int> nb = returnNeighbours();
            if (nb == null)
                return false;
            return nb.Contains(port);
        }
        public static void printRoutingTable()
        {

            Console.WriteLine(dis[0][0] + " " + dis[0][1] + " local");
            foreach (NDisEntry entry in ndis)
            {
                entry.print();
            }
        }
    }

}
