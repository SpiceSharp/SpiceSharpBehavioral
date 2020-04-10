using SpiceSharp.Behaviors;
using System;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharpBehavioral.Builders;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharp.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        private readonly OnePort<double> _variables;
        private readonly ElementSet<double> _elements;
        private readonly Func<double> _value;
        
        /// <summary>
        /// The functions that compute the derivatives.
        /// </summary>
        protected readonly Tuple<VariableNode, IVariable<double>, Func<double>>[] Functions;

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The instantaneous current")]
        public double Current { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("The instantaneous voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, BehavioralComponentContext context) : base(name)
        {
            var bp = context.GetParameterSet<BaseParameters>();
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));

            // Build the functions using our variable
            var df = context.Derivatives;

            // TODO: Take this from parameters
            var builder = new FunctionBuilder
            {
                Variables = new Dictionary<VariableNode, IVariable<double>>(),
                FunctionDefinitions = FunctionBuilderHelper.Defaults
            };

            foreach (var pair in df)
            {
                switch (pair.Key.NodeType)
                {
                    case NodeTypes.Voltage: builder.Variables.Add(pair.Key, state.GetSharedVariable(pair.Key.Name)); break;
                    case NodeTypes.Current: builder.Variables.Add(pair.Key, context.Branches[pair.Key].GetValue<IBranchedBehavior<double>>().Branch); break;
                    default:
                        throw new Exception("Invalid variable");
                }
            }

            // Let's build the derivative functions and get their matrix locations/rhs locations
            _value = builder.Build(bp.Function);
            Functions = new Tuple<VariableNode, IVariable<double>, Func<double>>[df.Count];
            var matLocs = new MatrixLocation[df.Count * 2];
            var rhsLocs = _variables.GetRhsIndices(state.Map);
            int index = 0;
            foreach (var pair in df)
            {
                var variable = builder.Variables[pair.Key];
                var func = builder.Build(pair.Value);
                Functions[index] = Tuple.Create(pair.Key, variable, func);
                matLocs[index * 2] = new MatrixLocation(rhsLocs[0], state.Map[variable]);
                matLocs[index * 2 + 1] = new MatrixLocation(rhsLocs[1], state.Map[variable]);
                index++;
            }

            // Get the matrix elements
            _elements = new ElementSet<double>(state.Solver, matLocs, rhsLocs);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            double[] values = new double[Functions.Length * 2 + 2];
            var total = Current = _value();

            int i;
            for (i = 0; i < Functions.Length; i++)
            {
                var df = Functions[i].Item3.Invoke();
                total -= Functions[i].Item2.Value * df;
                values[i * 2] = df;
                values[i * 2 + 1] = -df;
            }
            values[i * 2] = -total;
            values[i * 2 + 1] = total;
            _elements.Add(values);
        }
    }
}
