using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Numerics;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior" />
    /// <seealso cref="IFrequencyBehavior" />
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly int _posNode, _negNode, _branch;
        private readonly ElementSet<Complex> _elements, _coreElements;
        private readonly Func<double>[] _funcs;
        private readonly int[] _nodes;
        private readonly IComplexSimulationState _complex;

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
        public Complex ComplexCurrent => _complex.Solution[_branch];

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, BehavioralComponentContext context)
            : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();

            // Get the nodes
            var branch = context.Behaviors.GetValue<IBranchedBehavior>().Branch;
            _posNode = _complex.Map[context.Nodes[0]];
            _negNode = _complex.Map[context.Nodes[1]];
            _branch = _complex.Map[branch];

            // Get the variables from our behavioral description
            int index = 0;
            var locs = new MatrixLocation[context.ModelDescription.Count];
            _funcs = new Func<double>[context.ModelDescription.Count];
            _nodes = new int[context.ModelDescription.Count];
            foreach (var pair in context.ModelDescription)
            {
                _nodes[index] = _complex.Map[pair.Key];
                _funcs[index] = pair.Value;
                locs[index] = new MatrixLocation(_branch, _nodes[index]);
                index++;
            }

            // Get the matrix elements
            _elements = new ElementSet<Complex>(_complex.Solver, locs);
            _coreElements = new ElementSet<Complex>(_complex.Solver, new[] {
                new MatrixLocation(_branch, _posNode),
                new MatrixLocation(_branch, _negNode),
                new MatrixLocation(_posNode, _branch),
                new MatrixLocation(_negNode, _branch)
            });
        }

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            var values = new Complex[_funcs.Length];
            ComplexVoltage = new Complex();
            for (var i = 0; i < _funcs.Length; i++)
            {
                var df = _funcs[i].Invoke();
                ComplexVoltage += _complex.Solution[_nodes[i]] * df;
                values[i] = -df;
            }
            _elements.Add(values);
            _coreElements.Add(1.0, -1.0, 1.0, -1.0);
        }
    }
}
