using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetwProg
{
    public class NDisEntry
    {
        public int goal;
        Dictionary<int, int> disViaNb = new Dictionary<int, int>();

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
            KeyValuePair<int,int> tuple = getShortestNdis();
            Console.WriteLine(goal + " " + tuple.Value + " " + tuple.Key);
        }
        public KeyValuePair<int, int> getShortestNdis()
        {
            //NDIS KAN LEEG ZIJN, DAN FUCKT ALLES
            int value = int.MaxValue;
            KeyValuePair<int,int> temp = new KeyValuePair<int, int>();
            foreach (KeyValuePair<int,int> tuple in disViaNb)
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
}
