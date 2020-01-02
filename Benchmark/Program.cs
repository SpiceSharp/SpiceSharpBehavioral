using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Double;
using System;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new DoubleParser();
            Console.WriteLine(parser.Parse("0 ? 1"));

            Console.ReadKey();
        }
    }
}
