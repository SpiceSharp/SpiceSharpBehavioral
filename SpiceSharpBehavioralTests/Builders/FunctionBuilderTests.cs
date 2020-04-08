using NUnit.Framework;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            builder.Build(Node.Function("rnd", new Node[0]));
        }
    }
}
