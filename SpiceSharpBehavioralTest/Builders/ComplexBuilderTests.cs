using NUnit.Framework;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders.Direct;
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
            Assert.That(act.Real, Is.EqualTo(expected).Within(1e-20));
            Assert.That(act.Imaginary, Is.EqualTo(0.0).Within(1e-20));
        }

        [TestCaseSource(typeof(BuilderTestData), nameof(BuilderTestData.ComplexFunctionNodes))]
        public void When_BuildNodeFunctions_Expect_Reference(Node node, Complex expected)
        {
            var builder = new ComplexBuilder();
            builder.RegisterDefaultFunctions();
            var act = builder.Build(node);
            Assert.That(act.Real, Is.EqualTo(expected.Real).Within(1e-20));
            Assert.That(act.Imaginary, Is.EqualTo(expected.Imaginary).Within(1e-20));
        }

        [Test]
        public void When_BuildVariable_Expect_Reference()
        {
            var builder = new ComplexBuilder();
            var variable = new GetSetVariable<Complex>("a", Units.Volt);
            builder.VariableFound += (sender, args) =>
            {
                if (!args.Created && args.Node.Name == "a")
                    args.Result = variable.Value;
            };
            variable.Value = 2.0;
            var act = builder.Build(Node.Add(Node.Variable("a"), 3.0));
            Assert.That(act.Real, Is.EqualTo(5.0).Within(1e-20));
            Assert.That(act.Imaginary, Is.EqualTo(0.0).Within(1e-20));
        }
    }
}
