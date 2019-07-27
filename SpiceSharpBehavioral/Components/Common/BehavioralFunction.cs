using System;
using SpiceSharp;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A behavioral function that also can contain derivatives to variables.
    /// </summary>
    public class BehavioralFunction
    {
        private Dictionary<int, int> _map;
        private Derivatives<Func<double>> _derivatives;

        /// <summary>
        /// Gets the number of derivatives.
        /// </summary>
        public int DerivativeCount => _derivatives.Count;

        /// <summary>
        /// Gets the expression that returns the value of the function
        /// </summary>
        public Func<double> Value => _derivatives[0];

        /// <summary>
        /// Enumerates all derivatives of the behavioral function. The key is the index of the variable to which
        /// is derived.
        /// </summary>
        public IEnumerable<KeyValuePair<int, Func<double>>> Derivatives
        {
            get
            {
                foreach (var item in _map)
                {
                    var result = new KeyValuePair<int, Func<double>>(
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
        public BehavioralFunction(Dictionary<int, int> map, Derivatives<Func<double>> derivatives)
        {
            _map = map.ThrowIfNull(nameof(map));
            _derivatives = derivatives.ThrowIfNull(nameof(map));
        }
    }
}
