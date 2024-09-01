﻿using NUnit.Framework;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioralTest.Builders
{
    [TestFixture]
    public class ComplexFunctionBuilderTests
    {
        [Test]
        public void When_BuildConstant_Expect_Reference()
        {
            var builder = new ComplexFunctionBuilder();
            var act = builder.Build(Node.Constant(1.0)).Invoke();
            Assert.That(act.Real, Is.EqualTo(1.0).Within(1e-20));
            Assert.That(act.Imaginary, Is.EqualTo(0.0).Within(1e-20));
        }

        [TestCaseSource(typeof(BuilderTestData), nameof(BuilderTestData.Nodes))]
        public void When_BuildNode_Expect_Reference(Node node, double expected)
        {
            var builder = new ComplexFunctionBuilder();
            var act = builder.Build(node).Invoke();
            Assert.That(act.Real, Is.EqualTo(expected).Within(1e-20));
            Assert.That(act.Imaginary, Is.EqualTo(0.0).Within(1e-20));
        }

        [TestCaseSource(typeof(BuilderTestData), nameof(BuilderTestData.ComplexFunctionNodes))]
        public void When_BuildNodeFunctions_Expect_Reference(Node node, Complex expected)
        {
            var builder = new ComplexFunctionBuilder();
            builder.RegisterDefaultFunctions();
            var act = builder.Build(node).Invoke();
            Assert.That(act.Real, Is.EqualTo(expected.Real).Within(1e-20));
            Assert.That(act.Imaginary, Is.EqualTo(expected.Imaginary).Within(1e-20));
        }

        [Test]
        public void When_BuildVariable_Expect_Reference()
        {
            var builder = new ComplexFunctionBuilder();
            var variable = new GetSetVariable<Complex>("a", Units.Volt);
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && args.Node.Name == "a")
                    args.Variable = variable;
            };
            variable.Value = new Complex(1.0, 2.0);
            var act = builder.Build(Node.Variable("a") + 3.0).Invoke();
            Assert.That(act.Real, Is.EqualTo(4.0).Within(1e-20));
            Assert.That(act.Imaginary, Is.EqualTo(2.0).Within(1e-20));
        }

    }
}
