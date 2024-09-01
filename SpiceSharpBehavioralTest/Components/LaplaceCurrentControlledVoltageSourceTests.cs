using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpBehavioralTest.Components
{
    [TestFixture]
    public class LaplaceCurrentControlledVoltageSourceTests
    {
        [Test]
        public void When_Static_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "0", 1e3),
                new CurrentControlledVoltageSource("F1", "ref", "0", "V1", 2.0),
                new Resistor("Rref", "ref", "0", 1.0),

                new LaplaceCurrentControlledVoltageSource("H2", "act", "0", "V1", new[] { 2.0 }, new[] { 1.0 }));

            var op = new OP("op");
            var r = new RealVoltageExport(op, "ref");
            var a = new RealVoltageExport(op, "act");
            
            foreach (int _ in op.Run(ckt, OP.ExportOperatingPoint))
            {
                Assert.That(a.Value, Is.EqualTo(r.Value).Within(1e-12));
            }
        }

        [Test]
        public void When_TransientRC_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new Resistor("R1", "in", "out", 1e3),
                new Capacitor("C1", "out", "0", 1e-6),

                new VoltageSource("V2", "ai", "0", 0.0),
                new CurrentSource("I1", "0", "ai", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new LaplaceCurrentControlledVoltageSource("H1", "act", "0", "V2", new[] { 1.0 }, new[] { 1.0, 1.0e-3 }));

            var tran = new Transient("tran", 1e-7, 5e-6);
            var r = new RealVoltageExport(tran, "out");
            var a = new RealVoltageExport(tran, "act");

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(a.Value, Is.EqualTo(r.Value).Within(1e-12));
            }
        }

        [Test]
        public void When_TransientCR_Expect_Reference()
        {
            // High-pass RC filter
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new Capacitor("C1", "in", "out", 1e-6),
                new Resistor("R1", "out", "0", 1e3),

                new VoltageSource("V2", "ai", "0", 0.0),
                new CurrentSource("I1", "0", "ai", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new LaplaceCurrentControlledVoltageSource("F1", "act", "0", "V2", new[] { 0.0, 1.0e-3 }, new[] { 1.0, 1.0e-3 }));

            var tran = new Transient("tran", 1e-7, 5e-6);
            var r = new RealVoltageExport(tran, "out");
            var a = new RealVoltageExport(tran, "act");

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(a.Value, Is.EqualTo(r.Value).Within(1e-12));
            }
        }

        [Test]
        public void When_TransientLC_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new Inductor("L1", "in", "out", 1e-3),
                new Capacitor("C1", "out", "0", 1e-3),

                new VoltageSource("V2", "ai", "0", 0.0),
                new CurrentSource("I1", "0", "ai", new Pulse(0, 1, 1e-6, 1e-9, 1e-9, 1e-6, 2e-6)),
                new LaplaceCurrentControlledVoltageSource("F1", "act", "0", "V2", new[] { 1.0 }, new[] { 1.0, 0.0, 1.0e-6 }));

            var tran = new Transient("tran", 1e-7, 5e-6);
            var r = new RealVoltageExport(tran, "out");
            var a = new RealVoltageExport(tran, "act");

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(a.Value, Is.EqualTo(r.Value).Within(1e-12));
            }
        }

        [Test]
        public void When_ACRC_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "0", 1.0),
                new LaplaceCurrentControlledVoltageSource("F1", "act", "0", "V1", new[] { 1.0 }, new[] { 1.0, 1e-3 }));

            var ac = new AC("ac", new DecadeSweep(1.0, 1e6, 3));
            var a = new ComplexVoltageExport(ac, "act");

            foreach (int _ in ac.Run(ckt, AC.ExportSmallSignal))
            {
                var s = new Complex(0.0, ac.Frequency * 2 * Math.PI);
                Complex r = -1.0 / (1.0 + 1e-3 * s);
                Assert.That(a.Value.Real, Is.EqualTo(r.Real).Within(1e-20));
                Assert.That(a.Value.Imaginary, Is.EqualTo(r.Imaginary).Within(1e-20));
            }
        }
    }
}
