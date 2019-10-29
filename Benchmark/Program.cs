using System;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new Resistor("R1", "in", "0", 1),
                new BehavioralVoltageSource("E1", "out", "0", "Pow(V(in),3)"));

            var op = new DC("dc", "V1", -1, 1, 0.1);
            var export = new RealCurrentExport(op, "V1");
            op.ExportSimulationData += (sender, e) =>
            {
                Console.Write(export.Value);
                Console.Write(" -> ");
                Console.WriteLine(e.GetVoltage("out"));
            };
            op.Run(ckt);
            Console.ReadKey();
        }
    }
}
