using NUnit.Framework;
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
    }
}
