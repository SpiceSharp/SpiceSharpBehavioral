using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioralTests.Components
{
    public abstract class Framework
    {
        protected double AbsoluteTolerance = 1e-12;
        protected double RelativeTolerance = 1e-6;

        public void AnalyzeOp(Circuit ckt, OP op, IEnumerable<Export<double>> exports, IEnumerable<double> references)
        {
            op.ExportSimulationData += (sender, e) =>
            {
                var it = exports.GetEnumerator();
                var it2 = references.GetEnumerator();
                while (it.MoveNext())
                {
                    it2.MoveNext();
                    double expected = it2.Current;
                    double actual = it.Current.Value;
                    var tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * RelativeTolerance + AbsoluteTolerance;
                    Assert.AreEqual(expected, actual, tol);
                }
            };
            op.Run(ckt);
        }

        public void AnalyzeDC(Circuit ckt, DC dc, IEnumerable<Export<double>> exports, IEnumerable<Func<double>> references)
        {
            dc.ExportSimulationData += (sender, e) =>
            {
                var it = exports.GetEnumerator();
                var it2 = references.GetEnumerator();
                while (it.MoveNext())
                {
                    it2.MoveNext();
                    var expected = it2.Current();
                    var actual = it.Current.Value;
                    var tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * RelativeTolerance + AbsoluteTolerance;
                    Assert.AreEqual(expected, actual, tol);
                }
            };
            dc.Run(ckt);
        }

        public void AnalyzeAC(Circuit ckt, AC ac, IEnumerable<Export<Complex>> exports, IEnumerable<Func<Complex>> references)
        {
            ac.ExportSimulationData += (sender, e) =>
            {
                var it = exports.GetEnumerator();
                var it2 = references.GetEnumerator();
                while (it.MoveNext())
                {
                    it2.MoveNext();
                    var expected = it2.Current();
                    var actual = it.Current.Value;

                    var tol = Math.Max(Math.Abs(expected.Real), Math.Abs(actual.Real)) * RelativeTolerance + AbsoluteTolerance;
                    Assert.AreEqual(expected.Real, actual.Real, tol);

                    tol = Math.Max(Math.Abs(expected.Imaginary), Math.Abs(actual.Imaginary)) * RelativeTolerance + AbsoluteTolerance;
                    Assert.AreEqual(expected.Imaginary, actual.Imaginary, tol);
                }
            };
            ac.Run(ckt);
        }
    }
}
