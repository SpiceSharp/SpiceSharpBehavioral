using NUnit.Framework;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class DerivativesTests
    {
        private static void CompareDictionary(Dictionary<string, Node> expected, Dictionary<string, Node> actual)
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

        private static void Compare(Derivatives expected, Derivatives actual)
        {
            CompareDictionary(expected.Voltage, actual.Voltage);
            CompareDictionary(expected.Current, actual.Current);
        }

        [TestCaseSource(nameof(Expressions))]
        public void When_DeriveNode_Expect_Reference(Node function, Derivatives expected)
        {
            var d = new Deriver();
            var actual = d.Derive(function);
            Compare(expected, actual);
        }

        public static IEnumerable<TestCaseData> Expressions
        {
            get
            {
                var one = Node.Constant("1");
                var v1 = Node.Voltage("v1");
                var v2 = Node.Voltage("v2");
                var i1 = Node.Current("i1");

                yield return new TestCaseData(v1, new Derivatives(new Dictionary<string, Node> { { "v1", one } }, null)).SetName("{m}(v1)");
                yield return new TestCaseData(Node.Voltage("v2", "v1"), new Derivatives(new Dictionary<string, Node> { { "v1", Node.Minus(one) }, { "v2", one } }, null)).SetName("{m}(v1,v2)");
                yield return new TestCaseData(Node.Current("v1"), new Derivatives(null, new Dictionary<string, Node> { { "v1", one } })).SetName("{m}(iv1)");
                yield return new TestCaseData(Node.Plus(v1), new Derivatives(new Dictionary<string, Node> { { "v1", Node.Plus(one) } }, null)).SetName("{m}(+v1)");
                yield return new TestCaseData(Node.Minus(v1), new Derivatives(new Dictionary<string, Node> { { "v1", Node.Minus(one) } }, null)).SetName("{m}(-v1)");
                yield return new TestCaseData(Node.Not(v1), Derivatives.Empty).SetName("{m}(!v1)");
                yield return new TestCaseData(Node.Add(v1, v2), new Derivatives(new Dictionary<string, Node> { { "v1", one }, { "v2", one } }, null)).SetName("{m}(v1+v2)");
                yield return new TestCaseData(Node.Subtract(v1, Node.Current("V2")), 
                    new Derivatives(
                        new Dictionary<string, Node> { { "v1", one } }, 
                        new Dictionary<string, Node> { { "V2", Node.Minus(one) } })).SetName("{m}(v1-iV2)");
                yield return new TestCaseData(Node.Power(v1, v2), 
                    new Derivatives(new Dictionary<string, Node> 
                    { 
                        { "v1", Node.Multiply(v2, Node.Power(v1, Node.Subtract(v2, one))) },
                        { "v2", Node.Multiply(Node.Power(v1, v2), Node.Function("log", new[] { v1 })) }
                    }, null)).SetName("{m}(v1^v2)");
                yield return new TestCaseData(Node.Multiply(v1, v2),
                    new Derivatives(new Dictionary<string, Node> 
                    {
                        { "v1", v2 },
                        { "v2", v1 } 
                    }, null)).SetName("{m}(v1*v2)");
                yield return new TestCaseData(Node.Divide(v1, v2),
                    new Derivatives(
                        new Dictionary<string, Node>
                        {
                            { "v1", Node.Divide(one, v2) },
                            { "v2", Node.Minus(Node.Divide(one, Node.Power(v2, Node.Constant("2")))) }
                        }, null)).SetName("{m}(v1/v2)");
                yield return new TestCaseData(Node.Function("f", new[] { v1 }),
                    new Derivatives(
                        new Dictionary<string, Node> {
                            { "v1", Node.Function("df(0)", new[] { v1 }) }
                        }, null)).SetName("{m}(f_v1)");
            }
        }
    }
}
