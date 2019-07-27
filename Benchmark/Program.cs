using System;
using System.Linq.Expressions;
using System.Reflection;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new Resistor("R1", "in", "0", 1),
                new BehavioralVoltageSource("E1", "out", "0", "I(V1)*10"));

            var op = new OP("op");
            var export = new RealCurrentExport(op, "V1");
            op.ExportSimulationData += (sender, e) =>
            {
                Console.WriteLine(export.Value);
                Console.WriteLine(e.GetVoltage("out"));
            };
            op.Run(ckt);
            Console.ReadKey();
        }
    }
}
