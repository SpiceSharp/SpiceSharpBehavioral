using SpiceSharp.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Biasing behavior.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        private readonly List<Tuple<int, int, Func<double>>> _contributions = new List<Tuple<int, int, Func<double>>>();
        private readonly IBiasingSimulationState _biasing;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, BehavioralBindingContext context)
            : base(name)
        {
            var pd = context.GetParameterSet<IParserDescription>();
            _biasing = context.GetState<IBiasingSimulationState>();
            var parser = pd.Create();
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            throw new NotImplementedException();
        }
    }
}
