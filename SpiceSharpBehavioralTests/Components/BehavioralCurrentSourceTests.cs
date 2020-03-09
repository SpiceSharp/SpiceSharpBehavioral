using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharpBehavioralTests.Components
{
    [TestFixture]
    public class BehavioralCurrentSourceTests
    {
        [TestCaseSource(typeof(BehavioralCurrentSourceTestData), nameof(BehavioralCurrentSourceTestData.Op))]
        public void When_Op_Expect_Reference(string expression, double dcVoltage, double dcCurrent, double expected)
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", dcVoltage),
                new CurrentSource("I1", "in", "0", dcCurrent),
                new BehavioralCurrentSource("B1", "out", "0", expression),
                new Resistor("Rload", "out", "0", 1.0));
            var op = new OP("op");

            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
            };
            op.Run(ckt);
        }
    }

    public class BehavioralCurrentSourceTestData
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
    }
}
