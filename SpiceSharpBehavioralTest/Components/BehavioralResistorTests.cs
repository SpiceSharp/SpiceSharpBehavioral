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
        public void When_ComplexResitor_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "2", "0", 0.5),
                new VoltageSource("V2", "1", "0", 10),
                new BehavioralResistor("RSwitch", "1", "0", "v(2, 0) >= 1 ? 10 : (v(2, 0) <= 0 ? 1000000 : (exp(8.05904782547916 + 3 * -11.5129254649702 * (v(2, 0)-0.5)/(2*1) - 2 * -11.5129254649702 * pow(v(2, 0)-0.5, 3)/(pow(1,3)))))"));
            var op = new OP("Voltage switch simulation");
            var refExport = new RealCurrentExport(op, "V2");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(-0.00316228, refExport.Value, 1e-8);
            };

            op.Run(ckt);
        }

        [Test]
        public void When_SimpleResistor_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new Resistor("R1", "in", "out", 1e3),
                new BehavioralResistor("R2", "out", "0", "1k"));
            var dc = new DC("DC", "V1", -5, 5, 0.5);

            // Check power
            var refPower = new RealPropertyExport(dc, "R1", "p");
            var actPower = new RealPropertyExport(dc, "R2", "p");

            dc.ExportSimulationData += (sender, args) =>
            {
                var input = args.GetSweepValues()[0];
                var output = args.GetVoltage("out");
                Assert.AreEqual(input, output * 2, 1e-9);
                Assert.AreEqual(refPower.Value, actPower.Value, 1e-9);
            };
            dc.Run(ckt);
        }

        [Test]
        public void When_SimpleResistorFrequency_Expect_References()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 1e3),
                new BehavioralResistor("R2", "out", "0", "1k"));
            var ac = new AC("ac", new DecadeSweep(1, 1e3, 2));

            ac.ExportSimulationData += (sender, args) =>
            {
                var output = args.GetComplexVoltage("out");
                Assert.AreEqual(0.5, output.Real, 1e-9);
                Assert.AreEqual(0.0, output.Imaginary, 1e-9);
            };
            ac.Run(ckt);
        }

        [Test]
        public void When_ZeroDerivative_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "OUT", "0", 10),
                new BehavioralResistor("R1", "OUT", "0", "V(OUT,0)"));

            var op = new OP("op");
            var refExport = new RealCurrentExport(op, "V1");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(10.0, args.GetVoltage("OUT"), 1e-12);
                Assert.AreEqual(-1.0, refExport.Value, 1e-12);
            };
            op.Run(ckt);
        }

        [Test]
        public void When_If_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "2", "0", 0.5),
                new VoltageSource("V2", "1", "0", 10),
                new BehavioralResistor("RSwitch", "1", "0", "IF(v(2, 0) >= 1, 10, (v(2, 0) <= 0 ? 1000000 : (exp(8.05904782547916 + 3 * -11.5129254649702 * (v(2, 0)-0.5)/(2*1) - 2 * -11.5129254649702 * pow(v(2, 0)-0.5, 3)/(pow(1,3))))))")
                );
            var op = new OP("Voltage switch simulation");
            var refExport = new RealCurrentExport(op, "V2");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(-0.00316228, refExport.Value, 1e-8);
            };

            op.Run(ckt);
        }
    }
}
