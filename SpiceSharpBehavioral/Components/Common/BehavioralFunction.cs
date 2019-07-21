using SpiceSharp;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A behavioral function that also can contain derivatives to variables.
    /// </summary>
    public class BehavioralFunction
    {
        private Dictionary<int, int> _map;
        private ExpressionTreeDerivatives _derivatives;

        /// <summary>
        /// Gets the number of derivatives.
        /// </summary>
        public int DerivativeCount => _derivatives.Count;

        /// <summary>
        /// Gets the parameter that represents the current iteration solution.
        /// </summary>
        public ParameterExpression Solution { get; }

        /// <summary>
        /// Gets the expression that returns the value of the function
        /// </summary>
        public Expression Value => _derivatives[0];

        /// <summary>
        /// Enumerates all derivatives of the behavioral function. The key is the index of the variable to which
        /// is derived.
        /// </summary>
        public IEnumerable<KeyValuePair<int, Expression>> Derivatives
        {
            get
            {
                foreach (var item in _map)
                {
                    var result = new KeyValuePair<int, Expression>(
                        item.Key,
                        _derivatives[item.Value]
                        );
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BehavioralFunction"/> class.
        /// </summary>
        /// <param name="map">Maps each derivative to an unknown</param>
        /// <param name="derivatives">The derivatives.</param>
        public BehavioralFunction(Dictionary<int, int> map, ExpressionTreeDerivatives derivatives, ParameterExpression solution)
        {
            _map = map.ThrowIfNull(nameof(map));
            _derivatives = derivatives.ThrowIfNull(nameof(map));
            Solution = solution.ThrowIfNull(nameof(solution));
        }
    }
}
