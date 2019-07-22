# SpiceSharpBehavioral
This library extends Spice# components with behavioral sources for modelling electronics circuits.

## How does it work?
The parser uses Dijkstra's Shunting-Yard algorithm to parse the expressions into LINQ expression trees. It automatically constructs derivatives to other unknown variables (eg. "V(in)"), to be able to correctly load the Y-matrix and Rhs-vector each iteration. Still, there are some things it *cannot* do:

- **Unsolvable circuits** can occur. It becomes possible to bias circuits in impossible situations. For example, a component that does not dissipate power (but generates is), will cause the simulator to possibly throw cryptic exceptions as the circuit experiences a meltdown.
- **Unstable circuits**. Nonlinear devices are notorious when it comes to convergence or numerical stability. For example, an exponential curve is known to converge very slowly for diodes, so Spice# implements a number of "tricks" to aid convergence. Using this library means that these "tricks" are now also to the user to implement.

Please use the library with care.

## Usage

Including this library allows you to use two extra components:
- `BehavioralVoltageSource`
- `BehavioralCurrentSource`

```csharp
using System;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new BehavioralVoltageSource("A1", "out", "0", "V(in)^2+2"));

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
```

The parser is exposed as a parameter of the behavioral source. It is possible to register for parser events to extend its functionality. You have:
- `VariableFound` for defining variables that can be used.
- `FunctionFound` if you want to define mathematical functions in your expressions.
- `SpicePropertyFound` if you want to add address `V(...)`, `I(...)` or `@...[...]` expressions.

This example extends the parser to detect an exponential.
```csharp
using System;
using System.Linq.Expressions;
using System.Reflection;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;

namespace Example2
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
```

A list of differentiable functions have been implemented for convenience through the use of extension methods. Include the namespace `SpiceSharpBehavioral.Parsers.Helper` to enable the method `RegisterDefaultFunctions()` on a parser.

Defining a mathematical function will inevitably require you to also think about how its derivative should behave. If the derivative is chosen well, then the simulator will more likely converge quickly to the right value. The example also shows that you require access to LINQ expressions.
