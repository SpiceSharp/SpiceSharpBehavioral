using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpBehavioralTest.Components
{
    [TestFixture]
    public class BehavioralResistorTests
    {
        [Test]
        public void When_SimpleResistor_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new Resistor("R1", "in", "out", 1e3),
                new BehavioralResistor("R2", "out", "0", "1k")
                );
            var dc = new DC("DC", "V1", -5, 5, 0.5);

            dc.ExportSimulationData += (sender, args) =>
            {
                var input = args.GetSweepValues()[0];
                var output = args.GetVoltage("out");
                Assert.AreEqual(input, output * 2, 1e-9);
            };
            dc.Run(ckt);
        }
    }
}
