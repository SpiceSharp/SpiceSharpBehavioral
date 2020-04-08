using NUnit.Framework;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;

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
    }
}
