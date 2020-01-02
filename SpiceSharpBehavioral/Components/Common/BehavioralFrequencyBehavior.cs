using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Algebra;
using System.Numerics;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// Frequency behavior for a behavioral component.
    /// </summary>
    public abstract class BehavioralFrequencyBehavior : Behavior
    {
        /// <summary>
        /// Gets the state of the complex.
        /// </summary>
        /// <value>
        /// The state of the complex.
        /// </value>
        protected IComplexSimulationState ComplexState { get; }

        /// <summary>
        /// All contributions to the Y-matrix.
        /// </summary>
        private Complex[] _contributions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralFrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BehavioralFrequencyBehavior(string name, BehavioralBindingContext context)
            : base(name, context)
        {
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The binding context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get behaviors
            BiasingBehavior = context.GetBehavior<BehavioralBehavior>();

            State = ((FrequencySimulation)simulation).ComplexState;
            var solver = State.Solver;
            BuildFunctionMethod(solver);
        }

        /// <summary>
        /// Builds the action that can fill up the matrix.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="positive">The positive.</param>
        /// <param name="negative">The negative.</param>
        /// <returns>The action.</returns>
        protected Action Build(ISparseSolver<Complex> solver, Variable positive, Variable negative)
        {


            // Initialize contributions
            List<Action> _contributions = new List<Action>();
            {
                var func = Function.Value;
                _contributions.Add(() =>
                {
                    _cumulated = func();
                    CurrentValue = _cumulated;
                });
            }

            // Fill Y-matrix
            foreach (var item in Function.Derivatives)
            {
                var index = item.Key;
                var func = item.Value;

                // Ignore 0-contributions
                if (func == null)
                    continue;

                Element<double> posElt = null, negElt = null;
                if (posIndex > 0)
                    posElt = solver.GetElement(posIndex, index);
                if (negIndex > 0)
                    negElt = solver.GetElement(negIndex, index);

                if (posElt != null && negElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        posElt.Value += derivative;
                        negElt.Value -= derivative;
                        _cumulated -= BiasingState.Solution[index] * derivative;
                    });
                }
                else if (posElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        posElt.Value += derivative;
                        _cumulated -= BiasingState.Solution[index] * derivative;
                    });
                }
                else if (negElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        negElt.Value -= derivative;
                        _cumulated -= BiasingState.Solution[index] * derivative;
                    });
                }
            }

            // Fill Rhs-vectors
            Element<double> posRhs = null, negRhs = null;
            if (posIndex > 0)
                posRhs = solver.GetElement(posIndex);
            if (negIndex > 0)
                negRhs = solver.GetElement(negIndex);
            if (posRhs != null && negRhs != null)
                _contributions.Add(() =>
                {
                    posRhs.Value -= _cumulated;
                    negRhs.Value += _cumulated;
                });
            else if (posRhs != null)
                _contributions.Add(() => posRhs.Value -= _cumulated);
            else if (negRhs != null)
                _contributions.Add(() => negRhs.Value += _cumulated);

            // Build the total behavioral method, ignore if nothing has been done
            if (_contributions.Count == 0)
                return null;
            else
            {
                var actions = _contributions.ToArray();
                return () =>
                {
                    for (var i = 0; i < actions.Length; i++)
                        actions[i]();
                };
            }
        }

        /// <summary>
        /// Initialize small-signal parameters.
        /// </summary>
        public virtual void InitializeParameters()
        {
            _initializeMethod?.Invoke();
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        public virtual void Load()
        {
            _fillMatrix?.Invoke();
        }
    }
}
