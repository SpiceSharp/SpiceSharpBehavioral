using System;
using System.Linq.Expressions;
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
                new VoltageSource("V1", "in", "0", 0.0),
                new BehavioralVoltageSource("H1", "out", "0", "V(in)*100"));
            ckt["V1"].SetParameter("acmag", 1.0);

            var ac = new AC("ac", new LinearSweep(0, 1, 1));
            ac.ExportSimulationData += (sender, e) =>
            {
                Console.WriteLine(e.GetComplexVoltage("out"));
            };
            ac.Run(ckt);
            Console.ReadKey();
        }
    }
}
