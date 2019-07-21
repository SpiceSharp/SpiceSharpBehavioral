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
                new VoltageSource("V1", "in", "0", 0.0),
                new BehavioralVoltageSource("A1", "out", "0", "Exp(V(in))"));
            var parser = new ExpressionTreeDerivativeParser();
            parser.FunctionFound += (sender, e) =>
            {
                if (e.Name == "Exp")
                {
                    var arg = e[0];
                    var result = new ExpressionTreeDerivatives(arg.Count);
                    
                    // Index 0 contains the actual function value
                    result[0] = Expression.Call(typeof(Math).GetTypeInfo().GetMethod("Exp"), arg[0]);

                    // The other indices contain the derivatives, for which we need to apply the chain rule:
                    // f(g(x))' = f'(g(x)) * g'(x)
                    for (var i = 1; i < arg.Count; i++)
                        result[i] = arg[i] == null ? null : Expression.Multiply(arg[i], result[0]);
                    e.Result = result;
                }
            };
            ckt["A1"].SetParameter("parser", parser);

            var dc = new DC("dc", "V1", -1, 1, 0.1);
            dc.ExportSimulationData += (sender, e) =>
            {
                Console.WriteLine(e.GetVoltage("out"));
            };
            dc.Run(ckt);
            Console.ReadKey();
        }
    }
}
