using System;
using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;

namespace SpiceSharpBehavioral.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a behavioral voltage source.
    /// </summary>
    public class BiasingBehavior : BehavioralBehavior, IBiasingBehavior, IBranchedBehavior
    {
        private readonly int _pos, _neg, _branch;
        private readonly Action _fill;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Gets the branch equation variable.
        /// </summary>
        /// <value>
        /// The branch equation variable.
        /// </value>
        public Variable Branch { get; }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("current"), ParameterInfo("Voltage source current")]
        public double Current => BiasingState.Solution[_branch];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("power"), ParameterInfo("Instantaneous power")]
        public double Power => (BiasingState.Solution[_pos] - BiasingState.Solution[_neg]) * -BiasingState.Solution[_branch];

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("voltage"), ParameterInfo("Instantaneous voltage")]
        public double Voltage => CurrentValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, BehavioralBindingContext context) 
            : base(name, context) 
        {
            _pos = BiasingState.Map[context.Nodes[0]];
            _neg = BiasingState.Map[context.Nodes[1]];
            Branch = context.Variables.Create(Name.Combine("branch"), VariableType.Current);
            _branch = BiasingState.Map[Branch];
            _fill = Build(context.Variables.Ground, Branch);
            _elements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(_pos, _branch),
                new MatrixLocation(_neg, _branch),
                new MatrixLocation(_branch, _pos),
                new MatrixLocation(_branch, _neg));
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            _fill?.Invoke();
            _elements.Add(1.0, -1.0, 1.0, -1.0);
        }
    }
}
