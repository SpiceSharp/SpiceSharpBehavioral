using NUnit.Framework;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTests.Builders
{
    [TestFixture]
    public class DoubleBuilderTests
    {
        [TestCaseSource(typeof(DoubleBuilderTestData), nameof(DoubleBuilderTestData.Nodes))]
        public void When_BuildNode_Expect_Reference(Node node, double expected)
        {
            var builder = new DoubleBuilder();
            Assert.AreEqual(expected, builder.Build(node), 1e-20);
        }

        [TestCaseSource(typeof(DoubleBuilderTestData), nameof(DoubleBuilderTestData.FunctionNodes))]
        public void When_BuildNodeFunctions_Expect_Reference(Node node, double expected)
        {
            var builder = new DoubleBuilder();
            builder.FunctionDefinitions = DoubleBuilderHelper.Defaults;
            Assert.AreEqual(expected, builder.Build(node), 1e-20);
        }

        [Test]
        public void When_BuildVariable_Expect_Reference()
        {
            var builder = new DoubleBuilder();
            var variable = new GetSetVariable<double>("a", Units.Volt);
            builder.Variables = new Dictionary<string, IVariable<double>> { { "a", variable } };

            variable.Value = 2.0;
            Assert.AreEqual(5.0, builder.Build(Node.Add(Node.Variable("a"), Node.Constant("3"))), 1e-20);
        }

        [Test]
        public void When_BuildVoltage_Expect_Reference()
        {
            var builder = new DoubleBuilder();
            var node1 = new GetSetVariable<double>("n", Units.Volt);
            var node2 = new GetSetVariable<double>("r", Units.Volt);
            builder.Voltages = new Dictionary<string, IVariable<double>> { { "n", node1 }, { "r", node2 } };

            node1.Value = 1.0;
            node2.Value = 2.0;
            Assert.AreEqual(1.0, builder.Build(Node.Voltage("n")), 1e-20);
            Assert.AreEqual(-1.0, builder.Build(Node.Voltage("n", "r")), 1e-20);
        }

        [Test]
        public void When_BuildCurrent_Expect_Reference()
        {
            var builder = new DoubleBuilder();
            var branch = new GetSetVariable<double>("V1", Units.Ampere);
            builder.Currents = new Dictionary<string, IVariable<double>> { { "V1", branch } };

            branch.Value = 0.5;
            Assert.AreEqual(2.0, builder.Build(Node.Multiply(Node.Constant("4"), Node.Current("V1"))), 1e-20);
        }
    }
}
