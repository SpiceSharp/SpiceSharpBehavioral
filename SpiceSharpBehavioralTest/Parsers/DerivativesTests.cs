using NUnit.Framework;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTest.Parsers
{
    [TestFixture]
    public class DerivativesTests
    {
        private static void CompareDictionary<K, V>(Dictionary<K, V> expected, Dictionary<K, V> actual)
        {
            if (expected == null)
                Assert.AreEqual(null, actual);
            else
            {
                Assert.AreEqual(expected.Count, actual.Count);
                foreach (var pair in expected)
                    Assert.AreEqual(pair.Value, actual[pair.Key]);
            }
        }

        [TestCaseSource(nameof(Expressions))]
        public void When_DeriveNode_Expect_Reference(Node function, Dictionary<VariableNode, Node> expected)
        {
            var nf = new NodeFinder();
            var d = new Derivatives()
            {
                Variables = new HashSet<VariableNode>(nf.Build(function))
            };
            var actual = d.Derive(function);
            CompareDictionary(expected, actual);
        }

        public static IEnumerable<TestCaseData> Expressions
        {
            get
            {
                var v1 = VariableNode.Voltage("v1");
                var v2 = VariableNode.Voltage("v2");
                var v3 = VariableNode.Voltage("v3");
                var i1 = VariableNode.Current("i1");

                yield return new TestCaseData(v1, new Dictionary<VariableNode, Node> { { v1, Node.One } }).SetName("{m}(v1)");
                yield return new TestCaseData(Node.Voltage("v2", "v1"), new Dictionary<VariableNode, Node> { { v1, -Node.One }, { v2, Node.One } }).SetName("{m}(v1,v2)");
                yield return new TestCaseData(Node.Current("i1"), new Dictionary<VariableNode, Node> { { i1, Node.One } }).SetName("{m}(iv1)");
                yield return new TestCaseData(Node.Plus(v1), new Dictionary<VariableNode, Node> { { v1, +Node.One } }).SetName("{m}(+v1)");
                yield return new TestCaseData(Node.Minus(v1), new Dictionary<VariableNode, Node> { { v1, -Node.One } }).SetName("{m}(-v1)");
                yield return new TestCaseData(Node.Not(v1), null).SetName("{m}(!v1)");
                yield return new TestCaseData(Node.Add(v1, v2), new Dictionary<VariableNode, Node> { { v1, Node.One }, { v2, Node.One } }).SetName("{m}(v1+v2)");
                yield return new TestCaseData(Node.Subtract(v1, i1), new Dictionary<VariableNode, Node> { { v1, Node.One }, { i1, -Node.One } }).SetName("{m}(v1-i1)");
                yield return new TestCaseData(Node.And(v1, v2), null).SetName("{m}(v1&v2)");
                yield return new TestCaseData(Node.Or(v1, v2), null).SetName("{m}(v1|v2)");
                yield return new TestCaseData(Node.Xor(v1, v2), null).SetName("{m}(v1^v2)");
                yield return new TestCaseData(Node.Power(v1, v2), new Dictionary<VariableNode, Node>
                    {
                        { v1, v2 * Node.Power(v1, v2 - Node.One) },
                        { v2, Node.Function("log", v1) * Node.Power(v1, v2) }
                    }).SetName("{m}(v1^v2)");
                yield return new TestCaseData(Node.Multiply(v1, v2), new Dictionary<VariableNode, Node>
                    {
                        { v1, v2 },
                        { v2, v1 }
                    }).SetName("{m}(v1*v2)");
                yield return new TestCaseData(
                    Node.Function("abs", v1),
                    new Dictionary<VariableNode, Node> { { v1, Node.Function("sgn", v1) } }).SetName("{m}(abs v1)");
                yield return new TestCaseData(Node.Function("sgn", v1), null).SetName("{m}(sgn v1)");
                yield return new TestCaseData(
                    Node.Function("sqrt", v1), 
                    new Dictionary<VariableNode, Node> { { v1, 0.5 / Node.Function("sqrt", v1) } }).SetName("{m}(sqrt v1)");
                yield return new TestCaseData(
                    Node.Function("pwr", v1, v2),
                    new Dictionary<VariableNode, Node>
                    {
                        { v1, Node.Function("sgn", v1) * v2 * Node.Function("pwr", v1, v2 - Node.One) },
                        { v2, Node.Function("log", v1) * Node.Function("pwr", v1, v2) }
                    }).SetName("{m}(pwr v1 v2)");
                yield return new TestCaseData(Node.Equals(v1, v2), null).SetName("{m}(v1==v2)");
                yield return new TestCaseData(Node.NotEquals(v1, v2), null).SetName("{m}(v1==v2)");
                yield return new TestCaseData(Node.LessThan(v1, v2), null).SetName("{m}(v1==v2)");
                yield return new TestCaseData(Node.LessThanOrEqual(v1, v2), null).SetName("{m}(v1==v2)");
                yield return new TestCaseData(Node.GreaterThan(v1, v2), null).SetName("{m}(v1==v2)");
                yield return new TestCaseData(Node.GreaterThanOrEqual(v1, v2), null).SetName("{m}(v1==v2)");

                yield return new TestCaseData(Node.Function("atan2", v1, v2), new Dictionary<VariableNode, Node>
                {
                    { v1, v2 / (Node.Function("square", v1) + Node.Function("square", v2)) },
                    { v2, -v1 / (Node.Function("square", v1) + Node.Function("square", v2)) }
                }).SetName("{m}(atan2 v1 v2)");
                yield return new TestCaseData(Node.Function("hypot", v1, v2), new Dictionary<VariableNode, Node>
                {
                    { v1, 0.5 * v1 / Node.Function("hypot", v1, v2) },
                    { v2, 0.5 * v2 / Node.Function("hypot", v1, v2) }
                }).SetName("{m}(hypot v1 v2)");
                yield return new TestCaseData(
                    Node.Function("atanh", Node.Function("sin", v1)),
                    new Dictionary<VariableNode, Node> { 
                        {
                            v1, 
                            Node.Function("cos", v1) / (1 - Node.Function("square", Node.Function("sin", v1)))
                        }
                    }).SetName("{m}(atanh sin(v1))");
                yield return new TestCaseData(Node.Function("min", v1, v2), new Dictionary<VariableNode, Node>
                {
                    { v1, Node.Conditional(Node.LessThan(v1, v2), Node.Constant(1), Node.Zero)},
                    { v2, Node.Conditional(Node.LessThan(v1, v2), Node.Zero, Node.Constant(1))},
                }).SetName("{m}(min v1 v2)");

                yield return new TestCaseData(Node.Function("min", Node.Function("sin", v1), v2), new Dictionary<VariableNode, Node>
                {
                    { v1, Node.Conditional(Node.LessThan(Node.Function("sin", v1), v2), Node.Function("cos", v1), Node.Zero)},
                    { v2, Node.Conditional(Node.LessThan(Node.Function("sin", v1), v2), Node.Zero, Node.Constant(1))},
                }).SetName("{m}(min sin v1 v2)");

                yield return new TestCaseData(Node.Function("min", Node.Function("sin", v1), Node.Function("sin", v2)), new Dictionary<VariableNode, Node>
                {
                    { v1, Node.Conditional(Node.LessThan(Node.Function("sin", v1), Node.Function("sin", v2)), Node.Function("cos", v1), Node.Zero)},
                    { v2, Node.Conditional(Node.LessThan(Node.Function("sin", v1), Node.Function("sin", v2)), Node.Zero, Node.Function("cos", v2))},
                }).SetName("{m}(min sin v1 sin v2)");

                yield return new TestCaseData(Node.Function("max", v1, v2), new Dictionary<VariableNode, Node>
                {
                    { v1, Node.Conditional(Node.GreaterThan(v1, v2), Node.Constant(1), Node.Zero)},
                    { v2, Node.Conditional(Node.GreaterThan(v1, v2), Node.Zero, Node.Constant(1))},
                }).SetName("{m}(max v1 v2)");

                yield return new TestCaseData(Node.Function("max", Node.Function("sin", v1), v2), new Dictionary<VariableNode, Node>
                {
                    { v1, Node.Conditional(Node.GreaterThan(Node.Function("sin", v1), v2), Node.Function("cos", v1), Node.Zero)},
                    { v2, Node.Conditional(Node.GreaterThan(Node.Function("sin", v1), v2), Node.Zero, Node.Constant(1))},
                }).SetName("{m}(max sin v1 v2)");

                yield return new TestCaseData(Node.Function("max", Node.Function("sin", v1), Node.Function("sin", v2)), new Dictionary<VariableNode, Node>
                {
                    { v1, Node.Conditional(Node.GreaterThan(Node.Function("sin", v1), Node.Function("sin", v2)), Node.Function("cos", v1), Node.Zero)},
                    { v2, Node.Conditional(Node.GreaterThan(Node.Function("sin", v1), Node.Function("sin", v2)), Node.Zero, Node.Function("cos", v2))},
                }).SetName("{m}(max sin v1 sin v2)");

                yield return new TestCaseData(Node.Function("limit", v1, v2, v3), new Dictionary<VariableNode, Node>
                {
                    { v1, Node.Conditional(Node.And(Node.GreaterThan(v1, Node.Function("min", v2, v3)), Node.LessThan(v1, Node.Function("max", v2, v3))), Node.Constant(1), Node.Zero) },
                    { v2, Node.Conditional(Node.And(Node.LessThanOrEqual(v2, v3), Node.LessThanOrEqual(v1, Node.Function("min", v2, v3))), Node.Constant(1), Node.Zero) },
                    { v3, Node.Conditional(Node.And(Node.LessThan(v2, v3), Node.GreaterThanOrEqual(v1, Node.Function("max", v2, v3))), Node.Constant(1), Node.Zero) },

                }).SetName("{m}(limit v1 v2 v3)");
            }
        }
    }
}
