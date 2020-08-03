# SpiceSharpBehavioral
This library extends Spice# components with behavioral sources for modelling electronics circuits.

## Documentation
The API can be found [here](https://spicesharp.github.io/SpiceSharpBehavioral/)

## Quickstart

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

## Installation
Spice#.Behavioral is available as a NuGet package.

[![NuGet Badge](https://buildstats.info/nuget/spicesharpbehavioral)](https://www.nuget.org/packages/SpiceSharpBehavioral/) Spice#.Behavioral

## How does it work?
The parser parses expressions into functions. It automatically constructs derivatives to other unknown variables (eg. "V(in)"), to be able to correctly load the Y-matrix and Rhs-vector each iteration. Still, there are some things it *cannot* do:

- **Unsolvable circuits** can occur. It becomes possible to bias circuits in impossible situations. For example, a component that does not dissipate power (but generates is), will cause the simulator to possibly throw cryptic exceptions as the circuit experiences a meltdown.
- **Unstable circuits**. Nonlinear devices are notorious when it comes to convergence or numerical stability. For example, an exponential curve is known to converge very slowly for diodes, so Spice# implements a number of "tricks" to aid convergence. Using this library means that these "tricks" are now also to the user to implement.

Please use the library with care.

## Current build status

| Platform | Status |
|:---|-------:|
| Windows | ![Windows Tests](https://github.com/SpiceSharp/SpiceSharpBehavioral/workflows/Windows%20Tests/badge.svg) |
| Linux (Mono) | ![Linux Tests](https://github.com/SpiceSharp/SpiceSharpBehavioral/workflows/Linux%20Tests/badge.svg) |
| MacOS (Mono) | ![MacOS Tests](https://github.com/SpiceSharp/SpiceSharpBehavioral/workflows/MacOS%20Tests/badge.svg) |

## Aim of Spice#.Behavioral?

The aim is to provide an easier way of prototyping models in the Spice# simulator. While it is technically possible for anyone to extend Spice# with custom models and components to have full control over its behaviors, Spice#.Behavioral takes away a lot of that work.

Advantages:

- No prior knowledge needed about Newton-Raphson, Modified Nodal Analysis, etc.
- No need for calculating derivatives by hand for models.
- Changing models is likely easier and faster.

Disadvantages:

- General performance can be sub-optimal.
- It may be unclear for inexperienced users why a simulation became unstable or badly behaving.
