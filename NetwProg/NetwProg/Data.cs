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

        public static void AddDisEntry(int goal, int dist)
        {
            dis.Add(new int[2] { goal, dist });

            //NOTIFY CODE HERE

        }
    }

}
