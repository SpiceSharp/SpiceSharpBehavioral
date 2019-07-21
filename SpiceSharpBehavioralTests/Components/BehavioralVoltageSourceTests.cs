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
    }
}
