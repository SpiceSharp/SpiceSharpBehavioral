using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using System;

namespace SpiceSharpBehavioral.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    public class BiasingBehavior : BehavioralBehavior, IBiasingBehavior, 
        IParameterized<BaseParameters>
    {
        private readonly int _pos, _neg;
        private readonly Action _fill;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; }

        /// <summary>
        /// Gets the current applied by the source.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterInfo("Instantaneous current")]
        public double Current => CurrentValue;

        /// <summary>
        /// Gets the voltage over the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Instantaneous voltage")]
        public double Voltage => BiasingState.Solution[_pos] - BiasingState.Solution[_neg];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Instantaneous power")]
        public double GetPower() => (BiasingState.Solution[_pos] - BiasingState.Solution[_neg]) * CurrentValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, BehavioralBindingContext context) 
            : base(name, context)
        {
            _fill = Build(context.Nodes[0], context.Nodes[1]);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load() => _fill?.Invoke();
    }
}
