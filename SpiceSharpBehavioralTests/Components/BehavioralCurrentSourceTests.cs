using SpiceSharp;
using SpiceSharp.Components;
using NUnit.Framework;
using SpiceSharp.Simulations;
using System;
using System.Numerics;
using SpiceSharpBehavioral.Parsers;

namespace SpiceSharpBehavioralTests.Components
{
    [TestFixture]
    public class BehavioralCurrentSourceTests : Framework
    {
        [Test]
        public void When_SimpleBehavioralVoltageGain_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new BehavioralCurrentSource("H1", "out", "0", "V(in) + V(in) + 10 "),
                new Resistor("Rl", "out", "0", 1));
            ckt["H1"].SetParameter("parser", new SimpleDerivativeParser());

            var op = new OP("op");

            AnalyzeOp(ckt, op, new[] { new RealVoltageExport(op, "out") }, new[] { -12.0 });
        }

        [Test]
        public void When_SimpleBehavioralCurrentGain_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new Resistor("R1", "in", "0", 1),
                new BehavioralCurrentSource("H1", "out", "0", "I(V1)*10"),
                new Resistor("Rl", "out", "0", 1));

            var op = new OP("op");
            AnalyzeOp(ckt, op, new[] { new RealVoltageExport(op, "out") }, new[] { 10.0 });
        }

        [Test]
        public void When_CurrentSourceMultiplier_Expect_Reference()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "in", "0", 0.8),
                new Resistor("R1", "in", "0", 1e3),
                new BehavioralCurrentSource("H1", "out", "0", "I(I1)*2"),
                new Resistor("R2", "out", "0", 1e3));

            var op = new OP("op");
            AnalyzeOp(ckt, op, new[] { new RealPropertyExport(op, "R2", "i") }, new[] { -1.6 });
        }

        [Test]
        public void When_Subcircuit_Expect_Reference()
        {
            var subckt = new Circuit(
                new BehavioralCurrentSource("H1", "internal", "0", "V(in)+1"),
                new Resistor("R1", "internal", "out", 1e3));
            var ckt = new Circuit(
                new VoltageSource("V1", "a", "0", 0.5),
                new Resistor("R1", "b", "0", 1e3));

            // Instantiate the subcircuit
            var instance = new ComponentInstanceData(subckt, "x1");
            instance.NodeMap.Add("0", "0");
            instance.NodeMap.Add("in", "a");
            instance.NodeMap.Add("out", "b");
            ckt.Instantiate(instance);

            // Do simulation
            var op = new OP("op");
            AnalyzeOp(ckt, op, new[] { new RealVoltageExport(op, "b") }, new[] { -1500.0 });
        }

        [Test]
        public void When_SmallSignalAnalysis_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 4)
                    .SetParameter("acmag", 1.0),
                new BehavioralCurrentSource("H1", "out", "0", "V(in)^2 + 2"),
                new Resistor("R1", "out", "0", 1));

            // Do simulation
            var ac = new AC("ac");
            AnalyzeAC(ckt, ac, new[] { new ComplexVoltageExport(ac, "out") }, new Func<Complex>[] { () => new Complex(-8.0, 0.0) });
        }
    }
}
