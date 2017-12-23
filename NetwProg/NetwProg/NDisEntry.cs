using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetwProg
{
    /*
     * Een NDisEntry bevat een goal, welke gelijk is aan de key van Data.ndis. Ook bevat een NDisEntry een dictionary, waarvan de key de router is via welke
     * goal bereikt zou kunnen worden. De value van dit dictionary is de afstand via de neighbour die in de key staat naar 'goal'.
     */
    public class NDisEntry
    {
        public int goal;
        //          NB, DIST
        public Dictionary<int, int> disViaNb = new Dictionary<int, int>();

        public NDisEntry(int goal)
        {
            this.goal = goal;
        }

        public void AddPath(int nb, int dist)
        {
                if (disViaNb.ContainsKey(nb))
                    disViaNb[nb] = dist;
                else
                    disViaNb.Add(nb, dist);

                Data.Recompute();
        }

        public void removePath(int port)
        {
            disViaNb.Remove(port);
        }
        
        public List<int> returnAllNB()
        {
            List<int> temp = new List<int>();
            foreach (KeyValuePair<int, int> tuple in disViaNb)
            {
                temp.Add(tuple.Key);
            }
            return temp;
        }
        
        public void print()
        {
            if (goal == Program.port)
                Console.WriteLine(goal + " 0 local");
            else
            {
            KeyValuePair<int,int> tuple = getShortestNdis();
            Console.WriteLine(goal + " " + tuple.Value + " " + tuple.Key);
            }
        }

        public KeyValuePair<int, int> getShortestNdis()
        {
            //NDIS KAN LEEG ZIJN, DAN FUCKT ALLES
            int value = int.MaxValue;
            KeyValuePair<int, int> temp = new KeyValuePair<int, int>();
            lock (Data.computelock)
            {
                foreach (KeyValuePair<int, int> tuple in disViaNb)
                {
                    if (tuple.Value < value)
                    {
                        value = tuple.Value;
                        temp = tuple;
                    }
                }
                return temp;
            }
        }
        
        public int length()
        {
            return disViaNb.Keys.Count();
        }
        
    }
}
