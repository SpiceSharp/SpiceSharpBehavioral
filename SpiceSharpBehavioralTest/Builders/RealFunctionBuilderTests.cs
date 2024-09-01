using NUnit.Framework;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTest.Builders
{
    [TestFixture]
    public class RealFunctionBuilderTests
    {
        [TestCaseSource(typeof(BuilderTestData), nameof(BuilderTestData.Nodes))]
        public void When_BuildNode_Expect_Reference(Node node, double expected)
        {
            var builder = new RealFunctionBuilder();
            Assert.That(builder.Build(node).Invoke(), Is.EqualTo(expected).Within(1e-20));
        }

        [TestCaseSource(typeof(BuilderTestData), nameof(BuilderTestData.FunctionNodes))]
        public void When_BuildNodeFunctions_Expect_Reference(Node node, double expected)
        {
            var builder = new RealFunctionBuilder();
            builder.RegisterDefaultFunctions();
            Assert.That(builder.Build(node).Invoke(), Is.EqualTo(expected).Within(1e-20));
        }

        [Test]
        public void When_BuildRandomNode_Expect_NoException()
        {
            var builder = new RealFunctionBuilder();
            builder.RegisterDefaultFunctions();
            var func = builder.Build(Node.Function("rnd", new Node[0]));
            func();
        }

        [Test]
        public void When_FunctionCall_Expect_Reference()
        {
            var builder = new RealFunctionBuilder();
            var func1 = builder.Build(Node.Add(1.0, 2.0));
            builder.FunctionFound += (sender, args) =>
            {
                if (!args.Created && args.Function.Name == "f")
                {
                    args.ILState.Call(func1.Invoke);
                    args.Created = true;
                }
            };
            var func = builder.Build(Node.Function("f") * 2.0);
            Assert.That(func(), Is.EqualTo(6.0).Within(1e-20));
        }

        [Test]
        public void When_BuildVariable_Expect_Reference()
        {
            var builder = new RealFunctionBuilder();
            var variable = new GetSetVariable<double>("a", Units.Volt);
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && args.Node.Name == "a")
                    args.Variable = variable;
            };

            variable.Value = 2.0;
            Assert.That(builder.Build(Node.Variable("a") + 3.0).Invoke(), Is.EqualTo(5.0).Within(1e-20));
        }
    }
}
