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
    }
}
