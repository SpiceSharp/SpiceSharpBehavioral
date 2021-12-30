using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralSources
{
    /// <summary>
    /// A class for handling matrix/rhs contributions for behavioral components.
    /// </summary>
    /// <typeparam name="T">The base type value.</typeparam>
    public class BehavioralContributions<T>
    {
        private readonly Func<T>[] _derivatives;
        private readonly Func<T> _value;
        private readonly IVariable<T>[] _variables;
        private readonly T[] _values;
        private readonly ElementSet<T> _pElements, _nElements;
        private T _current;

        /// <summary>
        /// Gets the current value.
        /// </summary>
        public T Current
        {
            get
            {
                if (_value == null)
                {
                    T total = default;
                    for (var i = 0; i < _derivatives.Length; i++)
                        total = Accumulate(total, _variables[i].Value, _derivatives[i]());
                    return Invert(total);
                }
                else
                    return _current;
            }
        }

        /// <summary>
        /// Gets the number of contributions.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The function that accumulates the RHS result.
        /// </summary>
        public static Func<T, T, T, T> Accumulate { get; set; }

        /// <summary>
        /// The function to invert a value.
        /// </summary>
        public static Func<T, T> Invert { get; set; }

        /// <summary>
        /// Creates contributions for behavioral sources.
        /// </summary>
        /// <param name="state">The solver simulation state.</param>
        /// <param name="onePort">The one-port for row variables.</param>
        /// <param name="derivatives">The (column) variables and their derivatives.</param>
        /// <param name="value">The function that describes the value.</param>
        public BehavioralContributions(ISolverSimulationState<T> state, OnePort<T> onePort, List<(IVariable<T> Variables, Func<T> Derivative)> derivatives, Func<T> value)
            : this(state, onePort.Positive, onePort.Negative, derivatives, value)
        {
        }

        /// <summary>
        /// Creates contributions for behavioral sources.
        /// </summary>
        /// <param name="state">The solver simulation state.</param>
        /// <param name="posRow">The positive row variable.</param>
        /// <param name="negRow">The negative row variable.</param>
        /// <param name="derivatives">The (column) variables and their derivatives.</param>
        /// <param name="value">The function that describes the value.</param>
        public BehavioralContributions(ISolverSimulationState<T> state, IVariable<T> posRow, IVariable<T> negRow, List<(IVariable<T> Variable, Func<T> Derivative)> derivatives, Func<T> value)
        {
            _value = value;
            Count = derivatives.Count + (_value == null ? 0 : 1);
            _values = new T[Count];
            _derivatives = new Func<T>[derivatives.Count];
            _variables = new IVariable<T>[derivatives.Count];
            for (int i = 0; i < derivatives.Count; i++)
            {
                _derivatives[i] = derivatives[i].Derivative;
                _variables[i] = derivatives[i].Variable;
            }

            // Get the RHS locations
            int prhs = posRow != null ? state.Map[posRow] : 0;
            int nrhs = negRow != null ? state.Map[negRow] : 0;

            // Get the matrix locations
            var plocs = prhs > 0 ? new MatrixLocation[derivatives.Count] : null;
            var nlocs = nrhs > 0 ? new MatrixLocation[derivatives.Count] : null;
            for (int i = 0; i < derivatives.Count; i++)
            {
                var col = state.Map[_variables[i]];
                if (plocs != null)
                    plocs[i] = new(prhs, col);
                if (nlocs != null)
                    nlocs[i] = new(nrhs, col);
            }
            _pElements = plocs != null ? new ElementSet<T>(state.Solver, plocs, _value != null ? new[] { prhs } : null) : null;
            _nElements = nlocs != null ? new ElementSet<T>(state.Solver, nlocs, _value != null ? new[] { nrhs } : null) : null;
        }

        /// <summary>
        /// Calculates the contributions.
        /// </summary>
        public void Calculate()
        {
            if (Accumulate == null)
                CompileDefaultAccumulation();
            if (Invert == null)
                CompileDefaultInversion();

            // Calculates the values once such that they can be cached
            T total = _current = _value != null ? _value() : default;
            int i;
            for (i = 0; i < _derivatives.Length; i++)
            {
                var df = _derivatives[i]();
                total = Accumulate(total, _variables[i].Value, df);
                _values[i] = df;
            }
            if (_value != null)
                _values[i] = Invert(total);
        }

        /// <summary>
        /// Applies the last calculated contributions.
        /// </summary>
        public void Apply()
        {
            _pElements?.Add(_values);
            _nElements?.Subtract(_values);
        }

        /// <summary>
        /// Loads the behavioral contributions. This is effectively calculating and applying the contributions.
        /// </summary>
        public void Load()
        {
            Calculate();
            Apply();
        }

        private static void CompileDefaultAccumulation()
        {
            if (typeof(T) == typeof(double))
            {
                BehavioralContributions<double>.Accumulate = (total, value, df) => total - value * df;
                return;
            }
            if (typeof(T) == typeof(Complex))
            {
                BehavioralContributions<Complex>.Accumulate = (total, value, df) => total - value * df;
                return;
            }

            // Generic
            var total = Expression.Parameter(typeof(T));
            var value = Expression.Parameter(typeof(T));
            var df = Expression.Parameter(typeof(T));
            Accumulate = Expression.Lambda<Func<T, T, T, T>>(Expression.Subtract(total, Expression.Multiply(value, df)), total, value, df).Compile();
        }

        private static void CompileDefaultInversion()
        {
            if (typeof(T) == typeof(double))
            {
                BehavioralContributions<double>.Invert = d => -d;
                return;
            }
            if (typeof(T) == typeof(Complex))
            {
                BehavioralContributions<Complex>.Invert = d => -d;
            }

            // Generic
            var d = Expression.Parameter(typeof(T));
            Invert = Expression.Lambda<Func<T, T>>(Expression.Negate(d), d).Compile();
        }
    }
}
