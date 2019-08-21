using SpiceSharp.Components;
using SpiceSharp;
using NUnit.Framework;
using SpiceSharp.Simulations;

namespace SpiceSharpBehavioralTests.Components
{
    [TestFixture]
    public class BehavioralVoltageSourceTests : Framework
    {
        [Test]
        public void When_SimpleBehavioralVoltageGain_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new BehavioralVoltageSource("E1", "out", "0", "V(in)*10"));

            var op = new OP("op");

            AnalyzeOp(ckt, op, new[] { new RealVoltageExport(op, "out") }, new[] { 10.0 });
        }

        [Test]
        public void When_SimpleBehavioralCurrentGain_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new Resistor("R1", "in", "0", 1),
                new BehavioralVoltageSource("E1", "out", "0", "I(V1)*10"));

            var op = new OP("op");

            AnalyzeOp(ckt, op, new[] { new RealVoltageExport(op, "out") }, new[] { -10.0 });
        }

        [Test]
        public void When_CurrentSourceMultiplier_Expect_Reference()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "in", "0", 0.8),
                new Resistor("R1", "in", "0", 1e3),
                new BehavioralVoltageSource("E1", "out", "0", "I(I1)*2"));

            var op = new OP("op");
            AnalyzeOp(ckt, op, new[] { new RealVoltageExport(op, "out") }, new[] { 1.6 });
        }

        [Test]
        public void When_Subcircuit_Expect_Reference()
        {
            var subckt = new Circuit(
                new BehavioralVoltageSource("E1", "internal", "0", "V(in)+1"),
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
            AnalyzeOp(ckt, op, new[] { new RealVoltageExport(op, "b") }, new[] { 0.75 });
        }
    }
}
