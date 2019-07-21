using System;
using System.Collections.Generic;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// Method description
    /// </summary>
    public class MethodDescription
    {
        private static readonly MethodSignature DoubleSignature = new MethodSignature(typeof(double));
        private static readonly MethodSignature DoubleDoubleSignature = new MethodSignature(typeof(double), typeof(double));

        /// <summary>
        /// The aliases for this description
        /// </summary>
        private readonly Dictionary<MethodSignature, MethodInfo> _methods =
            new Dictionary<MethodSignature, MethodInfo>();

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the common target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public object Target { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescription"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MethodDescription(string name)
        {
            Name = name;
            Target = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescription"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="target">The target.</param>
        public MethodDescription(string name, object target)
        {
            Name = name;
            Target = target;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescription"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="target">The target.</param>
        /// <param name="method">The method.</param>
        public MethodDescription(string name, object target, MethodInfo method)
        {
            Name = name;
            Target = target;
            _methods.Add(new MethodSignature(method), method);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescription"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="target">The target.</param>
        /// <param name="methods">The methods.</param>
        public MethodDescription(string name, object target, MethodInfo[] methods)
        {
            Name = name;
            Target = target;
            foreach (var method in methods)
                _methods.Add(new MethodSignature(method), method);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescription"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public MethodDescription(string name, DoubleMethod method)
        {
            Name = name;
            Target = method.Target;
            _methods.Add(DoubleSignature, method.GetMethodInfo());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescription"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public MethodDescription(string name, DoubleDoubleMethod method)
        {
            Name = name;
            Target = method.Target;
            _methods.Add(DoubleDoubleSignature, method.GetMethodInfo());
        }

        /// <summary>
        /// Adds the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        public void Add(MethodInfo method)
        {
            _methods.Add(new MethodSignature(method), method);
        }

        /// <summary>
        /// Matches the specified signature to any of the method info's in the description.
        /// </summary>
        /// <param name="signature">The signature.</param>
        /// <returns></returns>
        public MethodInfo Match(MethodSignature signature)
        {
            // Quickly search for a direct match
            if (_methods.TryGetValue(signature, out var method))
                return method;
            throw new Exception($"No method by the name \"{Name}\" with this signature found");
        }
    }

    /// <summary>
    /// Delegate for a common function of the form y = f(x)
    /// </summary>
    /// <param name="x">The argument.</param>
    /// <returns></returns>
    public delegate double DoubleMethod(double x);

    /// <summary>
    /// Delegate for a common function of the form z = f(x, y)
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns></returns>
    public delegate double DoubleDoubleMethod(double x, double y);
}
