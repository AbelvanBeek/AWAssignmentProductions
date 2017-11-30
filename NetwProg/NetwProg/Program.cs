using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetwProg
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadInput();
        }

        static void ReadInput()
        {
            string[] input = Console.ReadLine().Split();
            Console.Title = "NetChange " + input[0];

            Data.dis.Add(new int[2] { int.Parse(input[0]), 0});
            for (int i = 1; i < input.Length; i++)
            {
                Data.AddDisEntry(int.Parse(input[i]), 1);
            }
        }
    }
}
