using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior" />
    /// <seealso cref="IFrequencyBehavior" />
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly int _posNode, _negNode;
        private readonly ElementSet<Complex> _elements;
        private readonly Func<double>[] _funcs;
        private readonly int[] _nodes;
        private readonly IComplexSimulationState _complex;

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        public Complex ComplexCurrent { get; private set; }

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
            _posNode = _complex.Map[context.Nodes[0]];
            _negNode = _complex.Map[context.Nodes[1]];

            // Get the variables from our behavioral description
            int index = 0;
            var locs = new MatrixLocation[context.ModelDescription.Count * 2];
            _funcs = new Func<double>[context.ModelDescription.Count];
            _nodes = new int[context.ModelDescription.Count];
            foreach (var pair in context.ModelDescription)
            {
                _nodes[index] = _complex.Map[pair.Key];
                _funcs[index] = pair.Value;
                locs[index * 2] = new MatrixLocation(_posNode, _nodes[index]);
                locs[index * 2 + 1] = new MatrixLocation(_negNode, _nodes[index]);
                index++;
            }

            // Get the matrix elements
            _elements = new ElementSet<Complex>(_complex.Solver, locs);
        }

        void IFrequencyBehavior.InitializeParameters()
        {
        }

        void IFrequencyBehavior.Load()
        {
            Complex[] values = new Complex[_funcs.Length * 2];

            Complex total = new Complex();
            for (var i = 0; i < _funcs.Length; i++)
            {
                var df = _funcs[i].Invoke();
                total += _complex.Solution[_nodes[i]] * df;
                values[i * 2] = df;
                values[i * 2 + 1] = -df;
            }
            _elements.Add(values);
        }
    }
}
