using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using SpiceSharpBehavioral.Expressions.Operators;

namespace SpiceSharpBehavioral.Expressions.Parsers
{
    /// <summary>
    /// Default math behavior methods for parsers.
    /// </summary>
    public static class DefaultMath
    {
        /// <summary>
        /// Registers a variable.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="parser">The parser.</param>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public static void RegisterVariable<T>(this StandardArithmeticParser parser, string name, Func<T> method)
        {
            var target = method.Target != null ? Expression.Constant(method.Target) : null;
            parser.Variables[name] = Expression.Call(target, method.GetMethodInfo());
        }

        /// <summary>
        /// Registers a variable.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="name">The name.</param>
        /// <param name="target">The target.</param>
        /// <param name="method">The method.</param>
        /// <exception cref="Exception">Invalid variable method</exception>
        public static void RegisterVariable(this StandardArithmeticParser parser, string name, object target, MethodInfo method)
        {
            if (method.GetParameters().Length > 0)
                throw new Exception("Invalid variable method");
            var etarget = target != null ? Expression.Constant(target) : null;
            parser.Variables[name] = Expression.Call(etarget, method);
        }

        /// <summary>
        /// Registers the methods.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="type">The type.</param>
        public static void RegisterMethods(this StandardArithmeticParser parser, Type type)
        {
            var methods = type.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (!parser.Methods.TryGetValue(method.Name, out var description))
                {
                    description = new MethodDescription(method.Name);
                    parser.Methods.Add(method.Name, description);
                }
                description.Add(method);
            }
        }

        /// <summary>
        /// Registers the methods.
        /// </summary>
        /// <param name="parser">The parser</param>
        /// <param name="instance">The instance.</param>
        public static void RegisterMethods(this StandardArithmeticParser parser, object instance)
        {
            var methods = instance.GetType().GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (!parser.Methods.TryGetValue(method.Name, out var description))
                {
                    description = new MethodDescription(method.Name);
                    parser.Methods.Add(method.Name, description);
                }
                description.Add(method);
            }
        }

        /// <summary>
        /// Registers the default Math methods with the specified parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public static void RegisterDefaults(this SpiceParser parser)
        {
            parser.RegisterMethods(typeof(Math));

            // Also allow Ln as an alias to Log
            if (!parser.Methods.TryGetValue("Ln", out var description))
            {
                description = new MethodDescription("Log");
                parser.Methods.Add("Ln", description);
            }
            description.Add(typeof(Math).GetTypeInfo().GetMethod("Log", new [] {typeof(double)}));
        }

        /// <summary>
        /// Registers the default Complex methods with the specified parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public static void RegisterComplexDefaults(this SpiceParser parser)
        {
            parser.RegisterMethods(typeof(Complex));

            // Also allow Ln as an alias to Log
            if (!parser.Methods.TryGetValue("Ln", out var description))
            {
                description = new MethodDescription("Log");
                parser.Methods.Add("Ln", description);
            }
            description.Add(typeof(Complex).GetTypeInfo().GetMethod("Log", new [] {typeof(Complex)}));
        }

        /// <summary>
        /// Registers the default Math methods and their derivatives with the specified parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public static void RegisterDefaults(this SpiceDerivativeParser parser)
        {
            // Register the defaults
            ((SpiceParser) parser).RegisterDefaults();

            // Trigonometry
            parser.RegisterDerivativeRule("Sin", new MethodDescription("DSin", Math.Cos));
            parser.RegisterDerivativeRule("Asin", new MethodDescription("DAsin", DAsin));
            parser.RegisterDerivativeRule("Cos", new MethodDescription("DCos", DCos));
            parser.RegisterDerivativeRule("Acos", new MethodDescription("DAcos", DAcos));
            parser.RegisterDerivativeRule("Tan", new MethodDescription("DTan", DTan));
            parser.RegisterDerivativeRule("Atan", new MethodDescription("DAtan", DAtan));

            // Hyperbolic
            parser.RegisterDerivativeRule("Sinh", new MethodDescription("DSinh", Math.Cosh));
            parser.RegisterDerivativeRule("Cosh", new MethodDescription("DCosh", Math.Sinh));
            parser.RegisterDerivativeRule("Tanh", new MethodDescription("DTanh", DTanh));

            // Powers
            parser.RegisterDerivativeRule("Exp", new MethodDescription("DExp", Math.Exp));
            parser.RegisterDerivativeRule("Log", new MethodDescription("DLog", DLog));
            parser.RegisterDerivativeRule("Pow", new []
            {
                new MethodDescription("DPow1", DPow1),
                new MethodDescription("DPow2", DPow2)
            });
        }

        /// <summary>
        /// Derivative of the cosine.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double DCos(double x) => -Math.Sin(x);

        /// <summary>
        /// Derivative of the tangent
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double DTan(double x)
        {
            double a = Math.Cos(x);
            return 1.0 / (a * a);
        }

        /// <summary>
        /// Derivative of the arcsine.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double DAsin(double x) => 1 / Math.Sqrt(1 - x * x);

        /// <summary>
        /// Derivative of the arccosine.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double DAcos(double x) => -1 / Math.Sqrt(1 - x * x);

        /// <summary>
        /// Derivative of the arctangent.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double DAtan(double x) => 1.0 / (1.0 + x * x);

        /// <summary>
        /// Derivative of the hyperbolic tangent.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double DTanh(double x)
        {
            var a = Math.Tanh(x);
            return 1 - a * a;
        }

        /// <summary>
        /// Derivative of the natural logarithm.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double DLog(double x) => 1.0 / x;

        /// <summary>
        /// Derivative of the 10-base logarithm.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double DLog10(double x) => 1.0 / (Math.Log(10.0) * x);

        /// <summary>
        /// Derivative of a power to the base argument.
        /// (x^n)' = n * x^(n-1)
        /// </summary>
        /// <param name="a">The base.</param>
        /// <param name="b">The exponent.</param>
        /// <returns></returns>
        public static double DPow1(double a, double b) => b * Math.Pow(a, b - 1);

        /// <summary>
        /// Derivative of a power to the exponent argument.
        /// (a^x)' = Log(a) * a^x
        /// </summary>
        /// <param name="a">The base.</param>
        /// <param name="b">The exponent.</param>
        /// <returns></returns>
        public static double DPow2(double a, double b) => Math.Log(b) * Math.Pow(a, b);
    }
}
