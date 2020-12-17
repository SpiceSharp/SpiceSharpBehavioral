using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder for parsing expressions.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IBuilder<T>
    {
        /// <summary>
        /// Builds the specified value from the specified expression node.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <returns>The value.</returns>
        T Build(Node expression);
    }
}
