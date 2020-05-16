using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : Behavior, IBiasingBehavior, IBranchedBehavior<double>
    {
        private readonly OnePort<double> _variables;
        private readonly IVariable<double> _branch;
        private readonly ElementSet<double> _elements, _coreElements;
        private readonly Func<double> _value;

        IVariable<double> IBranchedBehavior<double>.Branch => _branch;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("The instantaneous voltage")]
        public double Voltage { get; private set; }

        /// <summary>
        /// The functions that compute the derivatives.
        /// </summary>
        protected Tuple<VariableNode, IVariable<double>, Func<double>>[] Functions;

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The instantaneous current")]
        public double Current => _branch.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, BehavioralComponentContext context) 
            : base(name)
        {
            var bp = context.GetParameterSet<BaseParameters>();
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));
            _branch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            // Build the functions using our variable
            var df = context.Derivatives;

            // TODO: Take this from parameters
            var variables = new Dictionary<VariableNode, IVariable<double>>();
            foreach (var pair in df)
            {
                switch (pair.Key.NodeType)
                {
                    case NodeTypes.Voltage: variables.Add(pair.Key, state.GetSharedVariable(pair.Key.Name)); break;
                    case NodeTypes.Current:
                        var container = context.Branches[pair.Key];
                        if (container == context.Behaviors)
                            variables.Add(pair.Key, _branch);
                        else
                            variables.Add(pair.Key, container.GetValue<IBranchedBehavior<double>>().Branch);
                        break;
                    default:
                        throw new Exception("Invalid variable");
                }
            }
            var builder = bp.BuilderFactory(variables);

            // Let's build the derivative functions and get their matrix locations/rhs locations
            _value = builder.Build(bp.Function);
            Functions = new Tuple<VariableNode, IVariable<double>, Func<double>>[df.Count];
            var matLocs = new MatrixLocation[df.Count];
            var rhsLocs = state.Map[_branch];
            int index = 0;
            foreach (var pair in df)
            {
                var variable = variables[pair.Key];
                var func = builder.Build(pair.Value);
                Functions[index] = Tuple.Create(pair.Key, variable, func);
                matLocs[index] = new MatrixLocation(rhsLocs, state.Map[variable]);
                index++;
            }

            // Get the matrix elements
            _elements = new ElementSet<double>(state.Solver, matLocs);
            int br = state.Map[_branch];
            int pos = state.Map[_variables.Positive];
            int neg = state.Map[_variables.Negative];
            _coreElements = new ElementSet<double>(state.Solver, new[] {
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br)
            }, new[] { br });
        }

        void IBiasingBehavior.Load()
        {
            double[] values = new double[Functions.Length];
            var total = Voltage = _value();

            int i;
            for (i = 0; i < Functions.Length; i++)
            {
                var df = Functions[i].Item3.Invoke();
                total -= Functions[i].Item2.Value * df;
                values[i] = -df;
            }
            _elements.Add(values);
            _coreElements.Add(1.0, -1.0, 1.0, -1.0, total);
        }
    }
}
