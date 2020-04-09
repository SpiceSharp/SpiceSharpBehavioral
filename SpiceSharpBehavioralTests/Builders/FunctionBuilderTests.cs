using NUnit.Framework;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTests.Builders
{
    [TestFixture]
    public class FunctionBuilderTests
    {
        [TestCaseSource(typeof(DoubleBuilderTestData), nameof(DoubleBuilderTestData.Nodes))]
        public void When_BuildNode_Expect_Reference(Node node, double expected)
        {
            var builder = new FunctionBuilder();
            Assert.AreEqual(expected, builder.Build(node).Invoke(), 1e-20);
        }

        [TestCaseSource(typeof(DoubleBuilderTestData), nameof(DoubleBuilderTestData.FunctionNodes))]
        public void When_BuildNodeFunctions_Expect_Reference(Node node, double expected)
        {
            var builder = new FunctionBuilder
            {
                FunctionDefinitions = FunctionBuilderHelper.Defaults
            };
            Assert.AreEqual(expected, builder.Build(node).Invoke(), 1e-20);
        }

        [Test]
        public void When_BuildRandomNode_Expect_NoException()
        {
            var builder = new FunctionBuilder
            {
                FunctionDefinitions = FunctionBuilderHelper.Defaults
            };
            var func = builder.Build(Node.Function("rnd", new Node[0]));
            func();
        }

        [Test]
        public void When_FunctionCall_Expect_Reference()
        {
            var builder = new FunctionBuilder();
            var func1 = builder.Build(Node.Add(Node.Constant("1"), Node.Constant("2")));
            builder.FunctionDefinitions = new Dictionary<string, ApplyFunction>
            {
                { "f", (fbi, args) => fbi.Call(func1.Invoke) }
            };
            var func = builder.Build(Node.Multiply(Node.Function("f", new Node[0]), Node.Constant("2")));
            Assert.AreEqual(6.0, func(), 1e-20);
        }

        [Test]
        public void When_BuildVariable_Expect_Reference()
        {
            var builder = new FunctionBuilder();
            var variable = new GetSetVariable<double>("a", Units.Volt);
            builder.Variables = new Dictionary<string, IVariable<double>> { { "a", variable } };

            variable.Value = 2.0;
            Assert.AreEqual(5.0, builder.Build(Node.Add(Node.Variable("a"), Node.Constant("3"))).Invoke(), 1e-20);
        }

        [Test]
        public void When_BuildVoltage_Expect_Reference()
        {
            var builder = new FunctionBuilder();
            var node1 = new GetSetVariable<double>("n", Units.Volt);
            var node2 = new GetSetVariable<double>("r", Units.Volt);
            builder.Voltages = new Dictionary<string, IVariable<double>> { { "n", node1 }, { "r", node2 } };

            node1.Value = 1.0;
            node2.Value = 2.0;
            Assert.AreEqual(1.0, builder.Build(Node.Voltage("n")).Invoke(), 1e-20);
            Assert.AreEqual(-1.0, builder.Build(Node.Voltage("n", "r")).Invoke(), 1e-20);
        }

        [Test]
        public void When_BuildCurrent_Expect_Reference()
        {
            var builder = new FunctionBuilder();
            var branch = new GetSetVariable<double>("V1", Units.Ampere);
            builder.Currents = new Dictionary<string, IVariable<double>> { { "V1", branch } };

            branch.Value = 0.5;
            Assert.AreEqual(2.0, builder.Build(Node.Multiply(Node.Constant("4"), Node.Current("V1"))).Invoke(), 1e-20);
        }
    }
}
