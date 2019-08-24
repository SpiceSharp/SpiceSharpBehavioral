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
    public abstract class BehavioralFrequencyBehavior : Behavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the biasing behavior.
        /// </summary>
        protected BehavioralBiasingBehavior BiasingBehavior { get; private set; }

        /// <summary>
        /// Gets or sets the row of positive contributions.
        /// </summary>
        protected int PosIndex { get; set; }

        /// <summary>
        /// Gets or sets the row of negative contributions.
        /// </summary>
        protected int NegIndex { get; set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected ComplexSimulationState State { get; private set; }

        /// <summary>
        /// All contributions to the Y-matrix.
        /// </summary>
        private Complex[] _contributions;
        private Action _fillMatrix;
        private Action _initializeMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralFrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name"></param>
        public BehavioralFrequencyBehavior(string name)
            : base(name)
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
            BiasingBehavior = context.GetBehavior<BehavioralBiasingBehavior>();

            State = ((FrequencySimulation)simulation).ComplexState;
            var solver = State.Solver;
            BuildFunctionMethod(solver);
        }

        /// <summary>
        /// Unsetup the behavior.
        /// </summary>
        public override void Unbind()
        {
            BiasingBehavior = null;
            State = null;
            _fillMatrix = null;
            _contributions = null;
            _initializeMethod = null;
        }

        /// <summary>
        /// Get equation pointers.
        /// </summary>
        /// <param name="solver">Gets the equation pointers.</param>
        private void BuildFunctionMethod(Solver<Complex> solver)
        {
            var block = new List<Action>();
            var init_block = new List<Action>();

            // If the contributions cancel each other, don't bother compiling
            if (PosIndex == NegIndex)
            {
                _initializeMethod = null;
                _fillMatrix = null;
                return;
            }

            // The biasing behavior has a nice series of derivatives that need to be
            // calculated here. Since it is a small-signal analysis, we don't need
            // the actual value, only the derivatives.
            int count = 0;
            _contributions = new Complex[BiasingBehavior.Function.DerivativeCount];
            foreach (var item in BiasingBehavior.Function.Derivatives)
            {
                // Ignore 0-contributions
                var index = item.Key;
                var func = item.Value;
                if (func == null)
                    continue;

                MatrixElement<Complex> posElt = null, negElt = null;
                if (PosIndex > 0)
                    posElt = solver.GetMatrixElement(PosIndex, index);
                if (NegIndex > 0)
                    negElt = solver.GetMatrixElement(NegIndex, index);
                if (posElt != null && negElt != null)
                {
                    var i = count; // Avoid referencing the original count variable
                    block.Add(() =>
                    {
                        posElt.Value += _contributions[i];
                        negElt.Value -= _contributions[i];
                    });
                }
                else if (posElt != null)
                {
                    var i = count;
                    block.Add(() =>
                    {
                        posElt.Value += _contributions[i];
                    });
                }
                else if (negElt != null)
                {
                    var i = count;
                    block.Add(() =>
                    {
                        negElt.Value -= _contributions[i];
                    });
                }

                // Let's also initialize it
                var i2 = count;
                init_block.Add(() => _contributions[i2] = func());
                count++;
            }

            // Compile the methods
            {
                var actions = init_block.ToArray();
                _initializeMethod = () =>
                {
                    for (var i = 0; i < actions.Length; i++)
                        actions[i]();
                };
            }
            {
                var actions = block.ToArray();
                _fillMatrix = () =>
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
