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
        [TestCaseSource(typeof(BehavioralVoltageSourceTestData), nameof(BehavioralVoltageSourceTestData.Op))]
        public void When_DirectOutputOp_Expect_Reference(string expression, double dcVoltage, double dcCurrent, double expected)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage),
                new CurrentSource("I1", "in", "0", dcCurrent),
                new BehavioralVoltageSource("B1", "out", "0", expression));
            var op = new OP("op");

            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
            };
            op.Run(ckt);
        }

        [TestCaseSource(typeof(BehavioralVoltageSourceTestData), nameof(BehavioralVoltageSourceTestData.Ac))]
        public void When_Ac_Expect_Reference(string expression, double dcVoltage, double acVoltage, double dcCurrent, double acCurrent, Func<Complex, Complex> expected)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage).SetParameter("acmag", acVoltage),
                new CurrentSource("I1", "in", "0", dcCurrent).SetParameter("acmag", acCurrent),
                new BehavioralVoltageSource("B1", "out", "0", expression));
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

            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
            };
            op.Run(ckt);
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
                yield return new TestCaseData("v(in)*i(V1)", 1.0, 2.0, 3.0, 4.0, new Func<Complex, Complex>(s => -10.0)); // Gain is (3.0
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
