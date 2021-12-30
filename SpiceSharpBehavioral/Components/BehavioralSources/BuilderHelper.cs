using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using SpiceSharp.Simulations.Variables;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralSources
{
    /// <summary>
    /// Helper methods for behavioral components.
    /// </summary>
    public static class BuilderHelper
    {
        private static readonly SIUnitDefinition _scalar = new SIUnitDefinition("scalar", new SIUnits());
        private static readonly SIUnitDefinition _kelvin = new SIUnitDefinition("K", new SIUnits(0, 0, 0, 0, 1, 0, 0));
        private static readonly SIUnitDefinition _seconds = new SIUnitDefinition("s", new SIUnits(1, 0, 0, 0, 0, 0, 0));
        private static readonly SIUnitDefinition _mho = new SIUnitDefinition("Mho", new SIUnits(3, -2, -1, 2, 0, 0, 0));
        private static readonly SIUnitDefinition _joulesPerKelvin = new SIUnitDefinition("J/K", new SIUnits(-2, 2, 1, 0, -1, 0, 0));
        private static readonly SIUnitDefinition _joulesSeconds = new SIUnitDefinition("J*s", new SIUnits(-1, 2, 1, 0, 0, 0, 0));

        /// <summary>
        /// A default method for registering variables with a real function builder.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        public static void RegisterDefaultBuilder(object sender, BuilderCreatedEventArgs<double> args)
        {
            var bp = args.Context.GetParameterSet<Parameters>();
            var context = args.Context;
            var builder = args.Builder;
            var variables = new Dictionary<string, IVariable<double>>(bp.VariableComparer);

            // Register the default functions
            builder.RegisterDefaultFunctions();

            // Temperature
            if (context.TryGetState<ITemperatureSimulationState>(out var tempState))
                variables.Add("temperature", new FuncVariable<double>("temperature", () => tempState.Temperature, _kelvin));

            // Time variable
            if (context.TryGetState<IIntegrationMethod>(out var method))
            {
                var time = context.GetState<ITimeSimulationState>();

                // Time variable
                variables.Add("time", new FuncVariable<double>("time", () => method.Time, _seconds));

                // Allow using a ddt/ddt_slope function
                var comparer = RealFunctionBuilderHelper.Defaults.Comparer;
                builder.FunctionFound += (sender, args) =>
                {
                    if (!args.Created)
                    {
                        // Take care of the actual value
                        if (comparer.Equals("ddt", args.Function.Name))
                        {
                            var derivative = method.CreateDerivative();
                            args.ILState.Call(value =>
                            {
                                derivative.Value = value;
                                derivative.Derive();
                                return derivative.Derivative;
                            }, args.Function.Arguments);
                            args.Created = true;
                        }
                        else if (comparer.Equals("idt", args.Function.Name))
                        {
                            var integral = method.CreateIntegral();
                            args.ILState.Call(value =>
                            {
                                // Don't integrate if we're doing DC analysis
                                if (time.UseDc)
                                    return integral.Value;
                                integral.Derivative = value;
                                integral.Integrate();
                                return integral.Value;
                            }, args.Function.Arguments);
                            args.Created = true;
                        }
                        // Take care of the derivative
                        else if (comparer.Equals("ddt_slope", args.Function.Name))
                        {
                            args.ILState.Call(value => value * method.Slope, args.Function.Arguments);
                            args.Created = true;
                        }
                        else if (comparer.Equals("idt_slope", args.Function.Name))
                        {
                            var config = context.GetSimulationParameterSet<BiasingParameters>();
                            args.ILState.Call(value => {
                                if (time.UseDc)
                                    return 1e12; // This is basically putting a large resistance in parallel...
                                return value / method.Slope;
                            }, args.Function.Arguments);
                            args.Created = true;
                        }
                    }
                };
            }
            else
            {
                // Time variable
                variables.Add("time", new ConstantVariable<double>("time", 0.0, _seconds));

                // Allow using a ddt/ddt_slope function
                var comparer = RealFunctionBuilderHelper.Defaults.Comparer;
                builder.FunctionFound += (sender, args) =>
                {
                    if (!args.Created)
                    {
                        if (comparer.Equals("ddt", args.Function.Name) || comparer.Equals("ddt_slope", args.Function.Name))
                        {
                            args.ILState.Push(0.0);
                            args.Created = true;
                        }
                        else if (comparer.Equals("idt", args.Function.Name))
                        {
                            args.ILState.Push(0.0);
                            args.Created = true;
                        }
                        else if (comparer.Equals("idt_slope", args.Function.Name))
                        {
                            args.ILState.Push(1e-12);
                            args.Created = true;
                        }
                    }
                };
            }

            // Iteration control
            if (context.TryGetState<IIterationSimulationState>(out var iterState))
            {
                variables.Add("gmin", new FuncVariable<double>("gmin", () => iterState.Gmin, _mho));
                variables.Add("sourcefactor", new FuncVariable<double>("sourcefactor", () => iterState.SourceFactor, _scalar));
            }

            // Small-signal
            if (context.TryGetState<IComplexSimulationState>(out var cplxState))
            {
                variables.Add("smallsig", new ConstantVariable<double>("smallsig", 1.0, _scalar));
            }
            else
            {
                variables.Add("smallsig", new ConstantVariable<double>("smallsig", 0.0, _scalar));
            }

            // Some standard constants
            variables.Add("pi", new ConstantVariable<double>("pi", Math.PI, _scalar));
            variables.Add("e", new ConstantVariable<double>("e", Math.Exp(1.0), _scalar));
            variables.Add("boltz", new ConstantVariable<double>("boltz", Constants.Boltzmann, _joulesPerKelvin));
            variables.Add("planck", new ConstantVariable<double>("planck", 6.626207004e-34, _joulesSeconds));
            variables.Add("echarge", new ConstantVariable<double>("echarge", Constants.Charge, Units.Coulomb));
            variables.Add("kelvin", new ConstantVariable<double>("kelvin", -Constants.CelsiusKelvin, _kelvin));

            // Register these variables
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && variables.TryGetValue(args.Node.Name, out var variable))
                    args.Variable = variable;
            };
        }

        /// <summary>
        /// A default method for registering variables with a real function builder.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        public static void RegisterDefaultBuilder(object sender, BuilderCreatedEventArgs<Complex> args)
        {
            var bp = args.Context.GetParameterSet<Parameters>();
            var context = args.Context;
            var builder = args.Builder;
            var variables = new Dictionary<string, IVariable<Complex>>(bp.VariableComparer);

            // Register the default functions
            builder.RegisterDefaultFunctions();

            // Temperature
            if (context.TryGetState<ITemperatureSimulationState>(out var tempState))
                variables.Add("temperature", new FuncVariable<Complex>("temperature", () => tempState.Temperature, _kelvin));

            // Time variable
            if (context.TryGetState<IIntegrationMethod>(out var method))
            {
                variables.Add("time", new FuncVariable<Complex>("time", () => method.Time, _seconds));
            }

            // Iteration control
            if (context.TryGetState<IIterationSimulationState>(out var iterState))
            {
                variables.Add("gmin", new FuncVariable<Complex>("gmin", () => iterState.Gmin, _mho));
                variables.Add("sourcefactor", new FuncVariable<Complex>("sourcefactor", () => iterState.SourceFactor, _scalar));
            }

            // AC analysis
            if (context.TryGetState<IComplexSimulationState>(out var cplxState))
            {
                var comparer = RealFunctionBuilderHelper.Defaults.Comparer;
                builder.FunctionFound += (sender, args) =>
                {
                    if (!args.Created)
                    {
                        if (comparer.Equals("ddt_slope", args.Function.Name))
                        {
                            args.ILState.Call(value => value * cplxState.Laplace, args.Function.Arguments);
                            args.Created = true;
                        }
                        else if (comparer.Equals("idt_slope", args.Function.Name))
                        {
                            args.ILState.Call(value => cplxState.Laplace.Imaginary.Equals(0.0) ? double.PositiveInfinity : value / cplxState.Laplace, args.Function.Arguments);
                            args.Created = true;
                        }
                    }
                };
                variables.Add("smallsig", new ConstantVariable<Complex>("smallsig", 1.0, _scalar));
            }
            else
            {
                variables.Add("smallsig", new ConstantVariable<Complex>("smallsig", 0.0, _scalar));
            }    

            // Some standard constants
            variables.Add("pi", new ConstantVariable<Complex>("pi", Math.PI, _scalar));
            variables.Add("e", new ConstantVariable<Complex>("e", Math.Exp(1.0), _scalar));
            variables.Add("boltz", new ConstantVariable<Complex>("boltz", Constants.Boltzmann, _joulesPerKelvin));
            variables.Add("planck", new ConstantVariable<Complex>("planck", 6.626207004e-34, _joulesSeconds));
            variables.Add("echarge", new ConstantVariable<Complex>("echarge", Constants.Charge, Units.Coulomb));
            variables.Add("kelvin", new ConstantVariable<Complex>("kelvin", -Constants.CelsiusKelvin, _kelvin));

            // Register these variables
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && variables.TryGetValue(args.Node.Name, out var variable))
                    args.Variable = variable;
            };
        }
    }
}
