using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Builders.Functions;
using System;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Time-dependent behavior for a <see cref="BehavioralCapacitor"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(BehavioralCapacitor), typeof(ITimeBehavior), 1)]
    public class TimeBehavior : BiasingBehavior,
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

        /// <summary>
        /// Gets the instantaneous capacitor.
        /// </summary>
        /// <value>The instantaneous capacitor.</value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The instantaneous current")]
        public double Current => _qcap.Derivative;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public TimeBehavior(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IBiasingSimulationState>();
            _method = context.GetState<IIntegrationMethod>();
            _time = context.GetState<ITimeSimulationState>();

            _derivatives = new Func<double>[Derivatives.Count];
            _derivativeVariables = new IVariable<double>[Derivatives.Count];
            var builder = new RealFunctionBuilder();
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && DerivativeVariables.TryGetValue(args.Node, out var variable))
                    args.Variable = variable;
            };
            bp.RegisterBuilder(context, builder);
            var matLocs = new MatrixLocation[Derivatives.Count * 2];
            var rhsLocs = Variables.GetRhsIndices(state.Map);
            int index = 0;
            foreach (var pair in Derivatives)
            {
                _derivatives[index] = builder.Build(pair.Value);
                var variable = DerivativeVariables[pair.Key];
                _derivativeVariables[index] = variable;
                matLocs[index * 2] = new MatrixLocation(rhsLocs[0], state.Map[variable]);
                matLocs[index * 2 + 1] = new MatrixLocation(rhsLocs[1], state.Map[variable]);
                index++;
            }
            _value = builder.Build(Function);

            // Get the matrix elements
            _elements = new ElementSet<double>(state.Solver, matLocs, rhsLocs);

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
            _qcap.Integrate();
            var current = _qcap.Derivative;

            // _qcap.Derivative is the current as integrated by the current integration method
            double[] values = new double[_derivatives.Length * 2 + 2];
            int i;
            for (i = 0; i < _derivatives.Length; i++)
            {
                var g = _derivatives[i]() * _method.Slope;
                values[i * 2] = g;
                values[i * 2 + 1] = -g;
                current -= g * _derivativeVariables[i].Value;
            }
            values[i * 2] = -current;
            values[i * 2 + 1] = current;
            _elements.Add(values);
        }
    }
}
