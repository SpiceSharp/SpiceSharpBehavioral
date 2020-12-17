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
        [TestCaseSource(nameof(Op))]
        public void When_DirectOutputOp_Expect_Reference(string expression, double dcVoltage, double dcCurrent, double expected)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage),
                new CurrentSource("I1", "in", "0", dcCurrent),
                new BehavioralCurrentSource("B1", "0", "out", expression),
                new Resistor("Rload", "out", "0", 1.0));
            var op = new OP("op");

            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
            };
            op.Run(ckt);
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

            ac.ExportSimulationData += (sender, args) =>
            {
                var e = expected(args.Laplace);
                var a = args.GetComplexVoltage("out");
                Assert.AreEqual(e.Real, a.Real, 1e-12);
                Assert.AreEqual(e.Imaginary, a.Imaginary, 1e-12);
            };
            ac.Run(ckt);
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

            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
            };
            op.Run(ckt);
        }

        [Test]
        public void When_TimeDependent_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("Vtmp", "a", "0", 0.0), // Needed to have at least one independent source
                new BehavioralVoltageSource("V1", "in", "0", "5*sin(time*10*pi)"));
            var tran = new Transient("tran", 1e-3, 1);

            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(5 * Math.Sin(args.Time * 10 * Math.PI), args.GetVoltage("in"), 1e-9);
            };
            tran.Run(ckt);
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
