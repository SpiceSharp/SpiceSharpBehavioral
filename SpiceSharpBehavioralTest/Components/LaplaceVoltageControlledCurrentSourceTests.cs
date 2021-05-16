using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharpBehavioralTest.Components
{
    [TestFixture]
    public class LaplaceVoltageControlledCurrentSourceTests
    {
        [Test]
        public void When_Static_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageControlledCurrentSource("G1", "ref", "0", "in", "0", 2.0),
                new Resistor("Rref", "ref", "0", 1.0),

                new LaplaceVoltageControlledCurrentSource("G2", "act", "0", "in", "0", new[] { 2.0 }, new[] { 1.0 }),
                new Resistor("Ract", "act", "0", 1.0));

            var op = new OP("op");
            var r = new RealVoltageExport(op, "ref");
            var a = new RealVoltageExport(op, "act");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(r.Value, a.Value, 1e-12);
            };
            op.Run(ckt);
        }

        [Test]
        public void When_TransientRC_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new Resistor("R1", "in", "out", 1e3),
                new Capacitor("C1", "out", "0", 1e-6),

                new LaplaceVoltageControlledCurrentSource("G1", "0", "act", "in", "0", new[] { 1.0 }, new[] { 1.0, 1.0e-3 }),
                new Resistor("R2", "act", "0", 1.0));

            var tran = new Transient("tran", 1e-7, 5e-6);
            var r = new RealVoltageExport(tran, "out");
            var a = new RealVoltageExport(tran, "act");
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(r.Value, a.Value, 1e-12);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_TransientCR_Expect_Reference()
        {
            // High-pass RC filter
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new Capacitor("C1", "in", "out", 1e-6),
                new Resistor("R1", "out", "0", 1e3),

                new LaplaceVoltageControlledCurrentSource("G1", "0", "act", "in", "0", new[] { 0.0, 1.0e-3 }, new[] { 1.0, 1.0e-3 }),
                new Resistor("R2", "act", "0", 1.0));

            var tran = new Transient("tran", 1e-7, 5e-6);
            var r = new RealVoltageExport(tran, "out");
            var a = new RealVoltageExport(tran, "act");
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(r.Value, a.Value, 1e-12);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_TransientLC_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new Inductor("L1", "in", "out", 1e-3),
                new Capacitor("C1", "out", "0", 1e-3),

                new LaplaceVoltageControlledCurrentSource("G1", "0", "act", "in", "0", new[] { 1.0 }, new[] { 1.0, 0.0, 1.0e-6 }),
                new Resistor("R2", "act", "0", 1.0));

            var tran = new Transient("tran", 1e-7, 5e-6);
            var r = new RealVoltageExport(tran, "out");
            var a = new RealVoltageExport(tran, "act");
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(r.Value, a.Value, 1e-12);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_ACRC_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                new LaplaceVoltageControlledCurrentSource("G1", "0", "act", "in", "0", new[] { 1.0 }, new[] { 1.0, 1e-3 }),
                new Resistor("R1", "act", "0", 1.0));

            var ac = new AC("ac", new DecadeSweep(1.0, 1e6, 3));
            var a = new ComplexVoltageExport(ac, "act");
            ac.ExportSimulationData += (sender, args) =>
            {
                var s = args.Laplace;
                Complex r = 1.0 / (1.0 + 1e-3 * s);
                Assert.AreEqual(r.Real, a.Value.Real, 1e-20);
                Assert.AreEqual(r.Imaginary, a.Value.Imaginary, 1e-20);
            };
            ac.Run(ckt);
        }
    }
}
