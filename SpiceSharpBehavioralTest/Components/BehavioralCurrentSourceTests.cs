using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioralTest.Components
{
    [TestFixture]
    public class BehavioralCurrentSourceTests
    {
        [Test]
        public void When_DBSource_Expect_Reference()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "in1", "0", 1.0),
                new Resistor("R1", "in1", "0", 1.0),
                new BehavioralCurrentSource("I2", "in2", "0", "vdb(in1)"),
                new Resistor("R2", "in2", "0", 1.0));

            var op = new OP("op");
            op.Run(ckt);
        }

        [Test]
        public void When_DCSource_Expect_Reference()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "in1", "0", 1.0),
                new Resistor("R1", "in1", "0", 1.0),
                new BehavioralCurrentSource("I2", "in2", "0", "1"),
                new Resistor("R2", "in2", "0", 1.0));

            var op = new OP("op");

            // Check currents
            var refCurrent = new RealPropertyExport(op, "I1", "i");
            var actCurrent = new RealPropertyExport(op, "I2", "i");

            // Check voltages
            var refVoltage = new RealPropertyExport(op, "I1", "v");
            var actVoltage = new RealPropertyExport(op, "I2", "v");

            // Check powers
            var refPower = new RealPropertyExport(op, "I1", "p");
            var actPower = new RealPropertyExport(op, "I2", "p");

            // Do simulation
            foreach (int _ in op.Run(ckt, OP.ExportOperatingPoint))
            {
                Assert.That(actCurrent.Value, Is.EqualTo(refCurrent.Value).Within(1e-9));
                Assert.That(actVoltage.Value, Is.EqualTo(refVoltage.Value).Within(1e-9));
                Assert.That(actPower.Value, Is.EqualTo(refPower.Value).Within(1e-9));
            }
        }

        [TestCaseSource(nameof(Op))]
        public void When_DirectOutputOp_Expect_Reference(string expression, double dcVoltage, double dcCurrent, double expected)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage),
                new CurrentSource("I1", "in", "0", dcCurrent),
                new BehavioralCurrentSource("B1", "0", "out", expression),
                new Resistor("Rload", "out", "0", 1.0));
            var op = new OP("op");

            foreach (int _ in op.Run(ckt, OP.ExportOperatingPoint))
            {
                Assert.That(op.GetVoltage("out"), Is.EqualTo(expected).Within(1e-12));
            }
        }

        [TestCaseSource(nameof(Ac))]
        public void When_Ac_Expect_Reference(string expression, double dcVoltage, double acVoltage, double dcCurrent, double acCurrent, Func<Complex, Complex> expected)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage).SetParameter("acmag", acVoltage),
                new CurrentSource("I1", "in", "0", dcCurrent).SetParameter("acmag", acCurrent),
                new BehavioralCurrentSource("B1", "0", "out", expression),
                new Resistor("Rload", "out", "0", 1.0));
            var ac = new AC("op", new DecadeSweep(1, 1e3, 2));


            foreach (int _ in ac.Run(ckt, AC.ExportSmallSignal))
            {
                var e = expected(new Complex(0.0, ac.Frequency * 2.0 * Math.PI));
                var a = ac.GetComplexVoltage("out");
                Assert.That(a.Real, Is.EqualTo(e.Real).Within(1e-12));
                Assert.That(a.Imaginary, Is.EqualTo(e.Imaginary).Within(1e-12));
            }
        }

        [TestCaseSource(nameof(ImpedanceOp))]
        public void When_UsedAsImpedanceOp_Expect_Reference(string expression, double dcVoltage, double resistance, double expected)
        {
            // Use the behavioral current source as an impedance description
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage),
                new Resistor("R1", "in", "out", resistance),
                new BehavioralCurrentSource("B1", "out", "0", expression)
                );
            var op = new OP("op");

            foreach (int _ in op.Run(ckt, OP.ExportOperatingPoint))
            {
                Assert.That(op.GetVoltage("out"), Is.EqualTo(expected).Within(1e-12));
            }
        }

        [Test]
        public void When_DifferentialVoltage_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "a", "0", new Sine(0, 1, 100)),
                new VoltageSource("V2", "b", "0", new Sine(0, 2, 65)),
                new BehavioralCurrentSource("I1", "0", "out", "V(a,b)/2"),
                new Resistor("R1", "out", "0", 1));

            var tran = new Transient("tran", 1e-6, 1e-3);

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("out"), Is.EqualTo(0.5 * (tran.GetVoltage("a") - tran.GetVoltage("b"))).Within(1e-12));
            }
        }

        [Test]
        public void When_CapacitorTransient_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, 100)),
                new Capacitor("C1", "in", "0", 1e-6),
                new BehavioralCurrentSource("C2", "in", "0", "1u*ddt(V(in))"));
            var tran = new Transient("tran", 1e-6, 0.1);

            var reference = new RealPropertyExport(tran, "C1", "i");
            var actual = new RealPropertyExport(tran, "C2", "i");

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(actual.Value, Is.EqualTo(reference.Value).Within(1e-12));
            }
        }

        [Test]
        public void When_CapacitorAC_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, 100)),
                new Capacitor("C1", "in", "0", 1e-6),
                new BehavioralCurrentSource("C2", "in", "0", "1u*ddt(V(in))"));
            var ac = new AC("tran", new DecadeSweep(1, 1e6, 2));

            var reference = new ComplexPropertyExport(ac, "C1", "i");
            var actual = new ComplexPropertyExport(ac, "C2", "i");

            foreach (int _ in ac.Run(ckt, AC.ExportSmallSignal))
            {
                var r = reference.Value;
                var a = actual.Value;
                Assert.That(a.Real, Is.EqualTo(r.Real).Within(1e-12));
                Assert.That(a.Imaginary, Is.EqualTo(r.Imaginary).Within(1e-12));
            }
        }

        public static IEnumerable<TestCaseData> Op
        {
            get
            {
                yield return new TestCaseData("v(in)*3", 2.0, 0.0, 6.0);
                yield return new TestCaseData("v(in)*v(in)", 3.0, 0.0, 9.0);
                yield return new TestCaseData("i(V1)*2", 0.0, 2.5, -5.0);
                yield return new TestCaseData("v(in)*i(V1)", 2.0, 3.0, -6.0);
            }
        }
        public static IEnumerable<TestCaseData> Ac
        {
            get
            {
                yield return new TestCaseData("v(in)*3", 1.0, 0.0, 0.0, 0.0, new Func<Complex, Complex>(s => 0.0));
                yield return new TestCaseData("v(in)*3", 0.0, 1.0, 0.0, 0.0, new Func<Complex, Complex>(s => 3.0));
                yield return new TestCaseData("i(V1)*2", 0.0, 0.0, 1.0, 0.0, new Func<Complex, Complex>(s => 0.0));
                yield return new TestCaseData("i(V1)*2", 0.0, 0.0, 0.0, 1.0, new Func<Complex, Complex>(s => -2.0));
                yield return new TestCaseData("v(in)^2", 3.0, 1.0, 0.0, 0.0, new Func<Complex, Complex>(s => 6.0)); // Gain is (v^2)' = 2*v
                yield return new TestCaseData("v(in)*i(V1)", 1.0, 2.0, 3.0, 4.0, new Func<Complex, Complex>(s => -10.0)); // Gain is (3.0
            }
        }
        public static IEnumerable<TestCaseData> ImpedanceOp
        {
            get
            {
                yield return new TestCaseData("v(out)/1k", 1.0, 1.0e3, 0.5);
                yield return new TestCaseData("sin(v(out))", 1.0, 1.0, 0.51097342938856910952);
            }
        }
    }
}
