using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.ShuntingYard;
using System;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser<double>(new ShuntingYardDescription<double>(new DoubleOperators()));
            Console.WriteLine(parser.Parse("0 ? 1"));

            Console.ReadKey();
        }
    }
}
