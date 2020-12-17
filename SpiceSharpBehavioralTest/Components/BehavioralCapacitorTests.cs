using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpBehavioralTest.Components
{
    [TestFixture]
    public class BehavioralCapacitorTests
    {
        [Test]
        public void When_SimpleCapacitorTransient_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, 1e3)),
                new Capacitor("C1", "in", "0", 1e-3),
                new BehavioralCapacitor("C2", "in", "0", "1m*x"));
            var tran = new Transient("tran", 1e-6, 1e-3);

            var refExport = new RealPropertyExport(tran, "C1", "i");
            var actExport = new RealPropertyExport(tran, "C2", "i");
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(refExport.Value, actExport.Value, 1e-9);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_SimpleCapacitorFrequency_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0).SetParameter("acmag", 1.0),
                new Capacitor("C1", "in", "0", 1e-3),
                new BehavioralCapacitor("C2", "in", "0", "1m*x"));
            var ac = new AC("ac", new DecadeSweep(1, 1e6, 5));

            var refExport = new ComplexPropertyExport(ac, "C1", "i");
            var actExport = new ComplexPropertyExport(ac, "C2", "i");
            ac.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(refExport.Value.Real, actExport.Value.Real, 1e-9);
                Assert.AreEqual(refExport.Value.Imaginary, actExport.Value.Imaginary, 1e-9);
            };
            ac.Run(ckt);
        }
    }
}
