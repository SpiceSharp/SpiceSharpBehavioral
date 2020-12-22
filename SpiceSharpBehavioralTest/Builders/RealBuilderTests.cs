using NUnit.Framework;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTest.Builders
{
    [TestFixture]
    public class RealBuilderTests
    {
        [TestCaseSource(typeof(BuilderTestData), nameof(BuilderTestData.Nodes))]
        public void When_BuildNode_Expect_Reference(Node node, double expected)
        {
            var builder = new RealBuilder();
            Assert.AreEqual(expected, builder.Build(node), 1e-20);
        }

        [TestCaseSource(typeof(BuilderTestData), nameof(BuilderTestData.FunctionNodes))]
        public void When_BuildNodeFunctions_Expect_Reference(Node node, double expected)
        {
            var builder = new RealBuilder();
            builder.RegisterDefaultFunctions();
            Assert.AreEqual(expected, builder.Build(node), 1e-20);
        }

        [Test]
        public void When_BuildVariable_Expect_Reference()
        {
            var builder = new RealBuilder();
            var variable = new GetSetVariable<double>("a", Units.Volt);
            builder.VariableFound += (sender, args) =>
            {
                if (!args.Created && args.Node.Name == "a")
                    args.Result = variable.Value;
            };
            variable.Value = 2.0;
            Assert.AreEqual(5.0, builder.Build(Node.Add(Node.Variable("a"), 3.0)), 1e-20);
        }
    }
}
