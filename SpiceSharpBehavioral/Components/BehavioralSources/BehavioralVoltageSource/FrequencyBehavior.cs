using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Numerics;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.ParameterSets;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior" />
    /// <seealso cref="IFrequencyBehavior" />
    /// <seealso cref="IBranchedBehavior{T}"/>
    [BehaviorFor(typeof(BehavioralVoltageSource), typeof(IFrequencyBehavior), 1)]
    public class FrequencyBehavior : BiasingBehavior,
        IFrequencyBehavior,
        IBranchedBehavior<Complex>
    {
        private readonly OnePort<Complex> _variables;
        private readonly IVariable<Complex> _branch;
        private readonly ElementSet<Complex> _elements, _coreElements;
        private readonly Complex[] _values;

        /// <inheritdoc/>
        IVariable<Complex> IBranchedBehavior<Complex>.Branch => _branch;

        /// <summary>
        /// Gets the complex voltage.
        /// </summary>
        /// <value>
        /// The complex voltage.
        /// </value>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("The complex voltage")]
        public Complex ComplexVoltage { get; private set; }

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        [ParameterName("i"), ParameterName("i_c"), ParameterInfo("The complex current")]
        public Complex ComplexCurrent => _branch.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public FrequencyBehavior(BehavioralBindingContext context)
            : base(context)
        {
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));
            _branch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            var row = state.Map[_branch];
            var matLocs = new MatrixLocation[Functions.Length * 2];
            _values = new Complex[Functions.Length * 2];
            for (var i = 0; i < Functions.Length; i++)
            {
                IVariable<Complex> variable;
                switch (Functions[i].Item1.NodeType)
                {
                    case NodeTypes.Voltage: variable = state.GetSharedVariable(Functions[i].Item1.Name); break;
                    case NodeTypes.Current:
                        var container = context.Branches[Functions[i].Item1];
                        if (container == context.Behaviors)
                            variable = _branch;
                        else
                            variable = container.GetValue<IBranchedBehavior<Complex>>().Branch;
                        break;
                    default:
                        throw new Exception("Invalid variable");
                };
                matLocs[i] = new MatrixLocation(row, state.Map[variable]);
            }

            // Get the matrix elements
            _elements = new ElementSet<Complex>(state.Solver, matLocs);
            int br = state.Map[_branch];
            int pos = state.Map[_variables.Positive];
            int neg = state.Map[_variables.Negative];
            _coreElements = new ElementSet<Complex>(state.Solver, new[] {
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br)
            });
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            for (var i = 0; i < Functions.Length; i++)
            {
                var value = Functions[i].Item3.Invoke();
                _values[i] = -value;
            }
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            _elements.Add(_values);
            _coreElements.Add(1.0, -1.0, 1.0, -1.0);
        }
    }
}
