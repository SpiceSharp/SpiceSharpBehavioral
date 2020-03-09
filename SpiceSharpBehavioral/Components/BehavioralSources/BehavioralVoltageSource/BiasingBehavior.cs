using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        private readonly int _posNode, _negNode, _branch;
        private readonly ElementSet<double> _elements, _coreElements;
        private readonly Func<double> _value;
        private readonly Func<double>[] _funcs;
        private readonly int[] _nodes;
        private readonly IBiasingSimulationState _biasing;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        public double Voltage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, BehavioralComponentContext context) 
            : base(name)
        {
            _biasing = context.GetState<IBiasingSimulationState>();

            // Get the nodes
            var branch = context.Behaviors.GetValue<IBranchedBehavior>().Branch;
            _posNode = _biasing.Map[context.Nodes[0]];
            _negNode = _biasing.Map[context.Nodes[1]];
            _branch = _biasing.Map[branch];

            // Get the variables from our behavioral description
            int index = 0;
            var locs = new MatrixLocation[context.ModelDescription.Count];
            _funcs = new Func<double>[context.ModelDescription.Count];
            _value = context.ModelDescription.Value;
            _nodes = new int[context.ModelDescription.Count];
            foreach (var pair in context.ModelDescription)
            {
                _nodes[index] = _biasing.Map[pair.Key];
                _funcs[index] = pair.Value;
                locs[index] = new MatrixLocation(_branch, _nodes[index]);
                index++;
            }

            // Get the matrix elements
            _elements = new ElementSet<double>(_biasing.Solver, locs);
            _coreElements = new ElementSet<double>(_biasing.Solver, new[] {
                new MatrixLocation(_branch, _posNode),
                new MatrixLocation(_branch, _negNode),
                new MatrixLocation(_posNode, _branch),
                new MatrixLocation(_negNode, _branch)
            }, new[] { _branch });
        }

        void IBiasingBehavior.Load()
        {
            var values = new double[_funcs.Length];
            var total = Voltage = _value();
            for (var i = 0; i < _funcs.Length; i++)
            {
                var df = _funcs[i].Invoke();
                total -= _biasing.Solution[_nodes[i]] * df;
                values[i] = -df;
            }
            _elements.Add(values);
            _coreElements.Add(1.0, -1.0, 1.0, -1.0, total);
        }
    }
}
