using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealSimpleReleases
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(string arg in args)
            {
                Console.WriteLine(arg);
            }

            Console.Read();
        }
    }
}
