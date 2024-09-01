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
    public class BehavioralVoltageSourceTests
    {
        [Test]
        public void When_DCSource_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in1", "0", 1.0),
                new Resistor("R1", "in1", "0", 1.0),
                new BehavioralVoltageSource("V2", "in2", "0", "1"),
                new Resistor("R2", "in2", "0", 1.0));

            var op = new OP("op");

            // Check currents
            var refCurrent = new RealPropertyExport(op, "V1", "i");
            var actCurrent = new RealPropertyExport(op, "V2", "i");

            // Check voltages
            var refVoltage = new RealPropertyExport(op, "V1", "v");
            var actVoltage = new RealPropertyExport(op, "V2", "v");

            // Check powers
            var refPower = new RealPropertyExport(op, "V1", "p");
            var actPower = new RealPropertyExport(op, "V2", "p");

            // Do simulation
            foreach (int _ in op.Run(ckt, OP.ExportOperatingPoint))
            {
                Assert.That(actCurrent.Value, Is.EqualTo(refCurrent.Value).Within(1e-9));
                Assert.That(actVoltage.Value, Is.EqualTo(refVoltage.Value).Within(1e-9));
                Assert.That(actPower.Value, Is.EqualTo(refPower.Value).Within(1e-9));
            }
        }

        [TestCaseSource(typeof(BehavioralVoltageSourceTestData), nameof(BehavioralVoltageSourceTestData.Op))]
        public void When_DirectOutputOp_Expect_Reference(string expression, double dcVoltage, double dcCurrent, double expected)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage),
                new CurrentSource("I1", "in", "0", dcCurrent),
                new BehavioralVoltageSource("B1", "out", "0", expression));
            var op = new OP("op");

            foreach (int _ in op.Run(ckt, OP.ExportOperatingPoint))
            {
                Assert.That(op.GetVoltage("out"), Is.EqualTo(expected).Within(1e-12));
            }
        }

        [TestCaseSource(typeof(BehavioralVoltageSourceTestData), nameof(BehavioralVoltageSourceTestData.Ac))]
        public void When_Ac_Expect_Reference(string expression, double dcVoltage, double acVoltage, double dcCurrent, double acCurrent, Func<Complex, Complex> expected)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage).SetParameter("acmag", acVoltage),
                new CurrentSource("I1", "in", "0", dcCurrent).SetParameter("acmag", acCurrent),
                new BehavioralVoltageSource("B1", "out", "0", expression));
            var ac = new AC("op", new DecadeSweep(1, 1e3, 2));

            foreach (int _ in ac.Run(ckt, AC.ExportSmallSignal))
            {
                var e = expected(new Complex(0.0, ac.Frequency * 2.0 * Math.PI));
                var a = ac.GetComplexVoltage("out");
                Assert.That(a.Real, Is.EqualTo(e.Real).Within(1e-12));
                Assert.That(a.Imaginary, Is.EqualTo(e.Imaginary).Within(1e-12));
            }
        }

        [TestCaseSource(typeof(BehavioralVoltageSourceTestData), nameof(BehavioralVoltageSourceTestData.AdmittanceOp))]
        public void When_UsedAsAdmittanceOp_Expect_Reference(string expression, double dcVoltage, double resistance, double expected)
        {
            // Use the behavioral current source as an impedance description
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage),
                new Resistor("R1", "in", "out", resistance),
                new BehavioralVoltageSource("B1", "out", "0", expression)
                );
            var op = new OP("op");

            foreach (int _ in op.Run(ckt, OP.ExportOperatingPoint))
            {
                Assert.That(op.GetVoltage("out"), Is.EqualTo(expected).Within(1e-12));
            }
        }

        [Test]
        public void When_TimeDependent_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("Vtmp", "a", "0", 0.0), // Needed to have at least one independent source
                new BehavioralVoltageSource("V1", "in", "0", "5*sin(time*10*pi)"));
            var tran = new Transient("tran", 1e-3, 1);

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(5 * Math.Sin(tran.Time * 10 * Math.PI)).Within(1e-9));
            }
        }

        [Test]
        public void When_TimeDependentWithMaxFunction_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("Vtmp", "a", "0", 0.0), // Needed to have at least one independent source
                new VoltageSource("Vtmp2", "b", "0", 1.0),
                new BehavioralVoltageSource("V1", "in", "0", "max(0, V(b)*5*sin(time*10*pi))"));
            var tran = new Transient("tran", 1e-3, 1);

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(5 * Math.Max(0, Math.Sin(tran.Time * 10 * Math.PI))).Within(1e-9));
            }
        }

        [Test]
        public void When_TimeDependentWithMinFunction_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("Vtmp", "a", "0", 0.0), // Needed to have at least one independent source
                new VoltageSource("Vtmp2", "b", "0", 1.0), 
                new BehavioralVoltageSource("V1", "in", "0", "min(0, V(b)*5*sin(time*10*pi))"));
            var tran = new Transient("tran", 1e-3, 1);

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(5 * Math.Min(0, Math.Sin(tran.Time * 10 * Math.PI))).Within(1e-9));
            }
        }

        [Test]
        public void When_DerivativeTransient_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("Vtmp", "a", "0", new Sine(0, 1, 100)),
                new Capacitor("C1", "a", "0", 1.0), // this will derive the variable
                new BehavioralVoltageSource("V1", "in", "0", "ddt(V(a))"));
            var tran = new Transient("tran", 1e-3, 0.1);

            var expectedExport = new RealCurrentExport(tran, "Vtmp");
            var actualExport = new RealVoltageExport(tran, "in");

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                if (tran.Time > 0)
                {
                    var expected = expectedExport.Value;
                    var actual = -actualExport.Value;
                    Assert.That(actual, Is.EqualTo(expected).Within(1e-9));
                }
            }
        }

        [Test]
        public void When_IntegrateTransient_Expect_Reference()
        {
            var ckt = new Circuit(
                new CurrentSource("Itmp", "a", "0", new Sine(0, 1, 100)),
                new Capacitor("C1", "a", "0", 1.0), // this will integrate the variable
                new VoltageSource("Vtmp", "b", "0", new Sine(0, 1, 100)),
                new BehavioralVoltageSource("V1", "in", "0", "idt(V(b))"));
            var tran = new Transient("tran", 1e-3, 0.1);
            tran.TimeParameters.InitialConditions["a"] = 0.0;

            var expectedExport = new RealVoltageExport(tran, "a");
            var actualExport = new RealVoltageExport(tran, "in");

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                if (tran.Time > 0)
                {
                    var expected = expectedExport.Value;
                    var actual = -actualExport.Value;
                    Assert.That(actual, Is.EqualTo(expected).Within(1e-9));
                }
            }
        }

        [Test]
        public void When_CurrentSourceReference_Expect_Reference()
        {
            var ckt = new Circuit(
                new CurrentSource("I1", "a", "0", new Sine(0, 1, 100)),
                new Resistor("R1", "a", "0", 1e3),
                new BehavioralVoltageSource("V1", "b", "0", "I(I1)"));
            var tran = new Transient("tran", 1e-3, 0.1);

            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                var expected = Math.Sin(2 * Math.PI * 100 * tran.Time);
                var actual = tran.GetVoltage("b");
                Assert.That(actual, Is.EqualTo(expected).Within(1e-9));
            }
        }
    }

    public class BehavioralVoltageSourceTestData
    {
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
                yield return new TestCaseData("v(in)*i(V1)", 1.0, 2.0, 3.0, 4.0, new Func<Complex, Complex>(s => -10.0)); // Gain is 3.0
            }
        }
        public static IEnumerable<TestCaseData> AdmittanceOp
        {
            get
            {
                yield return new TestCaseData("i(B1)*1k", 1.0, 1.0e3, 0.5);
                yield return new TestCaseData("asin(i(B1))", 1.0, 1.0, 0.51097342938856910952);
            }
        }
    }
}
