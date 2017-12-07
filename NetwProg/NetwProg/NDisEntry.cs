using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetwProg
{
    public class NDisEntry
    {
        int goal;
        List<int[]> disViaNb;

        public NDisEntry(int goal)
        {
            this.goal = goal;
        }

        public void AddPath(int nb, int dist)
        {
            disViaNb.Add(new int[2] { nb, dist });
        }
        public List<int> returnAllNB()
        {
            List<int> temp = new List<int>();
            foreach (int[] tuple in disViaNb)
            {
                temp.Add(tuple[0]);
            }
            return temp;
        }
        public void print()
        {
            int[] tuple = getShortestNdis();
            Console.WriteLine(goal + " " + tuple[1] + " " + tuple[0]);
        }
        public int[] getShortestNdis()
        {
            int value = int.MaxValue;
            int[] temp = new int[2];
            foreach (int[] tuple in disViaNb)
            {
                if (tuple[1] < value)
                {
                    value = tuple[1];
                    temp = tuple;
                }
            }
            return temp;
        }
    }
}
