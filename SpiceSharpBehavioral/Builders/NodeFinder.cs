using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder that evaluates which nodes are in the expression.
    /// </summary>
    /// <seealso cref="BaseBuilder{T}" />
    /// <seealso cref="IBuilder{T}" />
    public class NodeFinder : IBuilder<List<string>>
    {
        /// <summary>
        /// Gets or sets the simulation that the builder should use.
        /// </summary>
        /// <value>
        /// The simulation.
        /// </value>
        public ISimulation Simulation { get; set; }

        /// <summary>
        /// Combines two node lists.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The combined list.</returns>
        private List<string> Combine(List<string> left, List<string> right)
        {
            if (left != null && right != null)
            {
                left.AddRange(right);
                return left;
            }
            else if (left != null)
                return left;
            else if (right != null)
                return right;
            else
                return null;
        }

        List<string> IBuilder<List<string>>.Add(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.Subtract(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.Multiply(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.Divide(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.Mod(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.Plus(List<string> argument) => argument;
        List<string> IBuilder<List<string>>.Minus(List<string> argument) => argument;
        List<string> IBuilder<List<string>>.Conditional(List<string> condition, List<string> ifTrue, List<string> ifFalse) => Combine(Combine(condition, ifTrue), ifFalse);
        List<string> IBuilder<List<string>>.Or(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.And(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.Equals(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.NotEquals(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.LessThan(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.GreaterThan(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.LessThanOrEqual(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.GreaterThanOrEqual(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.Pow(List<string> left, List<string> right) => Combine(left, right);
        List<string> IBuilder<List<string>>.CreateFunction(string name, IReadOnlyList<List<string>> arguments)
        {
            var result = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
                result = Combine(result, arguments[i]);
            return result;
        }


        /// <summary>
        /// Creates the variable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public List<string> CreateVariable(string name, QuantityTypes type) => null;

        /// <summary>
        /// Creates the value of a voltage.
        /// </summary>
        /// <param name="node">The name of the node.</param>
        /// <param name="reference">The name of the reference node.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the voltage.
        /// </returns>
        public List<string> CreateVoltage(string node, string reference, QuantityTypes type)
        {
            var n = new List<string>(2);
            n.Add(node);
            if (reference != null)
                n.Add(reference);
            return n;
        }

        /// <summary>
        /// Creates the value of a current.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the current.
        /// </returns>
        public List<string> CreateCurrent(string name, QuantityTypes type) => null;

        /// <summary>
        /// Creates a value of a property.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="property">The name of the property.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public List<string> CreateProperty(string name, string property, QuantityTypes type) => null;

        /// <summary>
        /// Creates the value of a number.
        /// </summary>
        /// <param name="value">The number.</param>
        /// <returns>
        /// The value of the number.
        /// </returns>
        public List<string> CreateNumber(string value) => null;
    }
}
