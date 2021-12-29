using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders.Functions;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Time-dependent behavior for a <see cref="BehavioralCapacitor"/>.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(BehavioralCapacitor)), AddBehaviorIfNo(typeof(ITimeBehavior))]
    [GeneratedParameters]
    public partial class Time : Biasing,
        IBiasingBehavior,
        ITimeBehavior
    {
        private readonly ElementSet<double> _elements;
        private readonly ITimeSimulationState _time;
        private readonly IIntegrationMethod _method;
        private readonly IDerivative _qcap;
        private readonly Func<double> _value;
        private readonly Func<double>[] _derivatives;
        private readonly IVariable<double>[] _derivativeVariables;
        private readonly double[] _values;

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Instantaneous current", Units = "A")]
        public double Current => _qcap.Derivative;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("Instantaneous voltage", Units = "V")]
        public double Voltage => Variables.Positive.Value - Variables.Negative.Value;

        /// <summary>
        /// Gets the power.
        /// </summary>
        /// <value>
        /// The power.
        /// </value>
        [ParameterName("p"), ParameterInfo("Instantaneous power", Units = "W")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IBiasingSimulationState>();
            _method = context.GetState<IIntegrationMethod>();
            _time = context.GetState<ITimeSimulationState>();

            var derivatives = new List<Func<double>>(Derivatives.Count);
            var derivativeVariables = new List<IVariable<double>>(Derivatives.Count);
            var builder = new RealFunctionBuilder();
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && VariableNodes.TryGetValue(args.Node, out var variable))
                    args.Variable = variable;
            };
            bp.RegisterBuilder(context, builder);
            var matLocs = new List<MatrixLocation>(Derivatives.Count * 2);
            var rhsLocs = Variables.GetRhsIndices(state.Map);
            foreach (var pair in Derivatives)
            {
                var variable = VariableNodes[pair.Key];
                if (state.Map.Contains(variable))
                {
                    derivatives.Add(builder.Build(pair.Value));
                    derivativeVariables.Add(variable);
                    matLocs.Add(new MatrixLocation(rhsLocs[0], state.Map[variable]));
                    matLocs.Add(new MatrixLocation(rhsLocs[1], state.Map[variable]));
                }
            }
            _value = builder.Build(Function);
            _derivatives = derivatives.ToArray();
            _values = new double[_derivatives.Length * 2 + 2];
            _derivativeVariables = derivativeVariables.ToArray();

            // Get the matrix elements
            _elements = new ElementSet<double>(state.Solver, matLocs.ToArray(), rhsLocs);

            // Create the derivative
            _qcap = _method.CreateDerivative();
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            _qcap.Value = _value();
        }

        /// <summary>
        /// Load behavior.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
                return;

            _qcap.Value = _value();
            _qcap.Derive();
            var current = _qcap.Derivative;

            // _qcap.Derivative is the current as integrated by the current integration method
            int i;
            for (i = 0; i < _derivatives.Length; i++)
            {
                var g = _derivatives[i]() * _method.Slope;
                _values[i * 2] = g;
                _values[i * 2 + 1] = -g;
                current -= g * _derivativeVariables[i].Value;
            }
            _values[i * 2] = -current;
            _values[i * 2 + 1] = current;
            _elements.Add(_values);
        }
    }
}
