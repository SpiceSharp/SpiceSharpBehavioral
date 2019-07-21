using System;
using System.Diagnostics;
using SpiceSharpBehavioral.Expressions;
using SpiceSharpBehavioral.Expressions.Parsers;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace Benchmark
{
    public static class BenchmarkBehavioralVsParser
    {
        public static void Run()
        {
            string expression = "1 + (x-1)*(x + Cos(x)) + Sqrt(x)";
            int count = 100000;
            
            EvaluateSpiceSharpParser(expression, count);
            EvaluateSpiceSharpBehavioral(expression, count);

            EvaluateSpiceSharpParser(expression, count);
            EvaluateSpiceSharpBehavioral(expression, count);

            expression = "(1+Pow(x,2)+0.5*Pow(x,3))/(1+x+Pow(x,2))";
            count = 10000;

            EvaluateSpiceSharpParser(expression, count);
            EvaluateSpiceSharpBehavioral(expression, count);

            Console.ReadKey();
        }

        private static void EvaluateSpiceSharpParser(string expression, int count)
        {
            Console.WriteLine("---------- Spice# Parser ----------");
            var sw = new Stopwatch();

            // Setup
            var parser = new SpiceSharpParser.Parsers.Expression.SpiceExpressionParser();
            var ectxt = new SpiceExpressionContext(SpiceExpressionMode.LtSpice);
            var pctxt = new ExpressionParserContext() {Functions = ectxt.Functions};
            var evctxt = new ExpressionEvaluationContext();

            // Parse
            ExpressionParseResult pr = null;
            for (var i = 0; i < 5; i++)
            {
                sw.Restart();
                pr = parser.Parse(expression, pctxt);
                sw.Stop();
                Console.WriteLine($"Parsing (run {i + 1}): {sw.ElapsedTicks} ({sw.ElapsedMilliseconds} ms)");
            }

            // Evaluate a first time
            evctxt.ExpressionContext.Parameters["x"] = new ConstantExpression(1.0);
            Console.WriteLine($"Evaluation: {pr.Value(evctxt)}");

            // Test bulk runs
            sw.Restart();
            for (var i = 0; i < count; i++)
                pr.Value(evctxt);
            sw.Stop();
            Console.WriteLine($"Execution: {sw.ElapsedTicks} ({sw.ElapsedMilliseconds} ms)");
            Console.WriteLine();
        }

        private static void EvaluateSpiceSharpBehavioral(string expression, int count)
        {
            Console.WriteLine("---------- Spice# Behavioral ----------");
            var sw = new Stopwatch();
            
            // Setup
            var parser = new SpiceParser();
            parser.RegisterDefaults();
            var evctxt = new ExpressionEvaluationContext(); // We don't want to test this speed in the benchmark, so let's replicate it.
            var dict = evctxt.ExpressionContext.Parameters;
            double X() => dict["x"].CurrentValue;
            parser.RegisterVariable("x", X);
            evctxt.ExpressionContext.Parameters.Add("x", new ConstantExpression(2.0));
            
            // Func<double> e = null;
            System.Linq.Expressions.Expression e = null;
            for (var i = 0; i < 5; i++)
            {
                sw.Restart();
                e = parser.Parse(expression, null);
                sw.Stop();
                Console.WriteLine($"Parsing (run {i + 1}): {sw.ElapsedTicks} ({sw.ElapsedMilliseconds} ms)");
            }
            
            // Compile
            sw.Restart();
            var f = System.Linq.Expressions.Expression.Lambda<Func<double>>(e).Compile();
            sw.Stop();
            Console.WriteLine($"Compilation: {sw.ElapsedTicks} ({sw.ElapsedMilliseconds} ms)");

            // Evaluate a first time
            dict["x"] = new ConstantExpression(1.0);
            Console.WriteLine($"Evaluation: {f()}");
            
            sw.Restart();
            for (var i = 0; i < count; i++)
                f();
            sw.Stop();
            Console.WriteLine($"Execution: {sw.ElapsedTicks} ({sw.ElapsedMilliseconds} ms)");
            Console.WriteLine();
        }
    }
}
