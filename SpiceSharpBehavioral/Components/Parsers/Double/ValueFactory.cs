using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.Components;
using SpiceSharp.Components.BehavioralBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioral.Components.Parsers.Double
{
    /// <summary>
    /// A value factory for behavioral components.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Parsers.Double.ValueFactory" />
    public class ValueFactory : SpiceSharpBehavioral.Parsers.Double.ValueFactory
    {
        private BehavioralBindingContext _context;

        /// <summary>
        /// Gets or sets the fudge factor.
        /// </summary>
        /// <value>
        /// The fudge factor.
        /// </value>
        [ParameterName("fudge"), ParameterInfo("The fudging factor for aiding convergence")]
        public double FudgeFactor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ValueFactory(BehavioralBindingContext context)
        {
            _context = context.ThrowIfNull(nameof(context));
            if (context.TryGetSimulationParameterSet(out BiasingParameters bp))
                FudgeFactor = bp.Gmin * 1e-20;
            else
                FudgeFactor = 1e-20;
        }

        /// <summary>
        /// Creates a function of the specified name.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The arguments of the function.</param>
        /// <returns>
        /// The value of the function.
        /// </returns>
        public override double CreateFunction(string name, double[] arguments)
        {
            // Override functions for components for improved convergence and more sense
            if (arguments.Length == 1)
            {
                switch (name)
                {
                    case "u":
                    case "U":
                        if (arguments[0] < 0)
                            return 0;
                        else if (arguments[0] > 0)
                            return 1;
                        else
                            return 0.5;
                    case "u2":
                    case "U2":
                        if (arguments[0] <= 0)
                            return 0;
                        else if (arguments[0] <= 1.0)
                            return arguments[0];
                        else
                            return 1;
                    case "uramp":
                    case "Uramp":
                        if (arguments[0] < 0)
                            return 0;
                        return arguments[0];
                    case "log":
                    case "Log":
                    case "ln":
                    case "Ln":
                        if (arguments[0] < 0)
                            return double.PositiveInfinity;
                        return Math.Log(arguments[0]);
                    case "log10":
                    case "Log10":
                        if (arguments[0] < 0)
                            return double.PositiveInfinity;
                        return Math.Log10(arguments[0]);
                }
            }
            if (arguments.Length == 2)
            {
                switch (name)
                {
                    case "pwr":
                    case "Pwr":
                        if (arguments[0].Equals(0.0) && arguments[1] < 0.0)
                            arguments[0] += FudgeFactor;
                        if (arguments[0] < 0.0)
                            return -Math.Pow(-arguments[0], arguments[1]);
                        else
                            return Math.Pow(arguments[0], arguments[1]);
                }
            }

            return base.CreateFunction(name, arguments);
        }

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public override double CreateProperty(PropertyType type, IReadOnlyList<string> arguments)
        {
            Variable node = null, reference = null;
            if (type == PropertyType.Property)
            {
                if (arguments.Count == 2 &&
                    _context.Behaviors.TryGetValue(arguments[0], out var container) &&
                    container.TryGetProperty(arguments[1], out double value))
                    return value;
                throw new Exception("Invalid property");
            }
            else if ((type & PropertyType.Voltage) != 0)
            {
                switch (arguments.Count)
                {
                    case 2: reference = _context.Variables.MapNode(arguments[1], VariableType.Voltage); goto case 1;
                    case 1: node = _context.Variables.MapNode(arguments[0], VariableType.Voltage); break;
                    default:
                        throw new Exception("Invalid voltage property");
                }
            }
            else if ((type & PropertyType.Current) != 0)
            {
                if (arguments.Count == 1 &&
                    _context.Behaviors.TryGetValue(arguments[0], out var container) &&
                    container.TryGetValue(out IBranchedBehavior branched))
                    node = branched.Branch;
                else
                    throw new Exception("Invalid current property.");
            }

            if ((type & PropertyType.ComplexMask) != 0)
            {
                if (_context.TryGetState(out IComplexSimulationState complex))
                {
                    Complex c = complex.Solution[complex.Map[node]];
                    if (reference != null)
                        c -= complex.Solution[complex.Map[reference]];
                    switch (type)
                    {
                        case PropertyType.VoltageReal:
                        case PropertyType.CurrentReal: return c.Real;
                        case PropertyType.VoltageImaginary:
                        case PropertyType.CurrentImaginary: return c.Imaginary;
                        case PropertyType.VoltageAmplitude:
                        case PropertyType.CurrentAmplitude: return c.Magnitude;
                        case PropertyType.VoltageDecibels:
                        case PropertyType.CurrentDecibels: return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                        case PropertyType.VoltagePhase:
                        case PropertyType.CurrentPhase: return c.Phase;
                    }
                }
            }
            else
            {
                if (_context.TryGetState(out IBiasingSimulationState biasing))
                {
                    double d = biasing.Solution[biasing.Map[node]];
                    if (reference != null)
                        d -= biasing.Solution[biasing.Map[reference]];
                    return d;
                }
            }
            throw new Exception("Unrecognized property");
        }
    }
}
