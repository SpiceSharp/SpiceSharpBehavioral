﻿using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders.Functions;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralCapacitor"/>.
    /// </summary>
    [BehaviorFor(typeof(BehavioralCapacitor)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly IComplexSimulationState _state;
        private readonly OnePort<Complex> _variables;
        private readonly ElementSet<Complex> _elements;
        private readonly Func<Complex>[] _derivatives;
        private readonly IVariable<Complex>[] _derivativeVariables;
        private readonly Complex[] _values;

        /// <summary>
        /// Gets the complex voltage.
        /// </summary>
        /// <value>The complex voltage.</value>
        [ParameterName("v"), ParameterInfo("The complex voltage", Units = "V")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        [ParameterName("i"), ParameterInfo("The complex current", Units = "A")]
        public Complex ComplexCurrent
        {
            get
            {
                Complex total = 0.0;
                for (var i = 0; i < _derivatives.Length; i++)
                    total += _derivatives[i]() * _derivativeVariables[i].Value;
                return total * _state.Laplace;
            }
        }

        /// <summary>
        /// Gets the complex power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("The complex power", Units = "W")]
        public Complex ComplexPower => ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public Frequency(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            _state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(
                _state.GetSharedVariable(context.Nodes[0]),
                _state.GetSharedVariable(context.Nodes[1]));

            var derivatives = new List<Func<Complex>>(Derivatives.Count);
            var derivativeVariables = new List<IVariable<Complex>>(Derivatives.Count);
            var builder = new ComplexFunctionBuilder();
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && DerivativeVariables.TryGetValue(args.Node, out var variable))
                    args.Variable = new FuncVariable<Complex>(variable.Name, () => variable.Value, variable.Unit);
            };
            bp.RegisterBuilder(context, builder);
            var matLocs = new List<MatrixLocation>(Derivatives.Count * 2);
            var rhsLocs = _variables.GetRhsIndices(_state.Map);
            foreach (var pair in Derivatives)
            {
                var variable = context.MapNode(_state, pair.Key);
                if (_state.Map.Contains(variable))
                {
                    derivatives.Add(builder.Build(pair.Value));
                    derivativeVariables.Add(variable);
                    matLocs.Add(new MatrixLocation(rhsLocs[0], _state.Map[variable]));
                    matLocs.Add(new MatrixLocation(rhsLocs[1], _state.Map[variable]));
                }
            }
            _derivatives = derivatives.ToArray();
            _values = new Complex[_derivatives.Length * 2];
            _derivativeVariables = derivativeVariables.ToArray();
            _elements = new ElementSet<Complex>(_state.Solver, matLocs.ToArray());
        }

        /// <summary>
        /// The frequency-dependent initialization.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// The frequency-dependent load behavior.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            for (var i = 0; i < _derivatives.Length; i++)
            {
                var g = _state.Laplace * _derivatives[i]();
                _values[i * 2] = g;
                _values[i * 2 + 1] = -g;
            }
            _elements.Add(_values);
        }
    }
}
