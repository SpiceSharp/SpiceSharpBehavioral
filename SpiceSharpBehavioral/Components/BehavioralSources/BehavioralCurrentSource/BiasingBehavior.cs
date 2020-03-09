using SpiceSharp.Behaviors;
using System;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        private readonly int _posNode, _negNode;
        private readonly ElementSet<double> _elements;
        private readonly Func<double> _value;
        private readonly Func<double>[] _funcs;
        private readonly int[] _nodes;
        private readonly IBiasingSimulationState _biasing;

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public double Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, BehavioralComponentContext context) : base(name)
        {
            _biasing = context.GetState<IBiasingSimulationState>();

            // Get the nodes
            _posNode = _biasing.Map[context.Nodes[0]];
            _negNode = _biasing.Map[context.Nodes[1]];

            // Get the variables from our behavioral description
            int index = 0;
            var locs = new MatrixLocation[context.ModelDescription.Count * 2];
            _funcs = new Func<double>[context.ModelDescription.Count];
            _value = context.ModelDescription.Value;
            _nodes = new int[context.ModelDescription.Count];
            foreach (var pair in context.ModelDescription)
            {
                _nodes[index] = _biasing.Map[pair.Key];
                _funcs[index] = pair.Value;
                locs[index * 2] = new MatrixLocation(_posNode, _nodes[index]);
                locs[index * 2 + 1] = new MatrixLocation(_negNode, _nodes[index]);
                index++;
            }
           
            // Get the matrix elements
            _elements = new ElementSet<double>(_biasing.Solver, locs, new int[] { _posNode, _negNode });
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            double[] values = new double[_funcs.Length * 2 + 2];
            Current = _value();
            var total = Current;

            int i;
            for (i = 0; i < _funcs.Length; i++)
            {
                var df = _funcs[i].Invoke();
                total -= _biasing.Solution[_nodes[i]] * df;
                values[i * 2] = -df;
                values[i * 2 + 1] = df;
            }
            values[i * 2] = total;
            values[i * 2 + 1] = -total;
            _elements.Add(values);
        }
    }
}
