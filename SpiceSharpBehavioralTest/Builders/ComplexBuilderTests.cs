using NUnit.Framework;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioralTest.Builders
{
    [TestFixture]
    public class ComplexBuilderTests
    {
        [TestCaseSource(typeof(BuilderTestData), nameof(BuilderTestData.Nodes))]
        public void When_BuildNode_Expect_Reference(Node node, double expected)
        {
            var builder = new ComplexBuilder();
            var act = builder.Build(node);
            Assert.AreEqual(expected, act.Real, 1e-20);
            Assert.AreEqual(0.0, act.Imaginary, 1e-20);
        }

        [Test]
        public void When_BuildVariable_Expect_Reference()
        {
            var builder = new ComplexBuilder();
            var variable = new GetSetVariable<Complex>("a", Units.Volt);
            builder.Variables = new Dictionary<VariableNode, IVariable<Complex>> { { VariableNode.Variable("a"), variable } };

            variable.Value = 2.0;
            var act = builder.Build(Node.Add(Node.Variable("a"), 3.0));
            Assert.AreEqual(5.0, act.Real, 1e-20);
            Assert.AreEqual(0.0, act.Imaginary, 1e-20);
        }
    }
}
