using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Components.Parsers.DoubleFunc
{
    /// <summary>
    /// A factory for values. This class will modify some default implementations of regular
    /// doubles to have an easier time with convergence.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Parsers.Double.ValueFactory" />
    public class ValueFactory : SpiceSharpBehavioral.Parsers.DoubleFunc.ValueFactory
    {
        private readonly BehavioralBindingContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ValueFactory(BehavioralBindingContext context)
        {
            _context = context.ThrowIfNull(nameof(context));
        }

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public override Func<double> CreateProperty(PropertyType type, IReadOnlyList<string> arguments)
        {
            Variable node = null, reference = null;
            if (type == PropertyType.Property)
            {
                if (arguments.Count == 2 &&
                    _context.Behaviors.TryGetBehaviors(arguments[0], out var container))
                {
                    var getter = container.CreatePropertyGetter<double>(arguments[1]);
                    if (getter != null)
                        return getter;
                }
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
                    _context.Behaviors.TryGetBehaviors(arguments[0], out var container) &&
                    container.TryGetValue(out IBranchedBehavior branched))
                    node = branched.Branch;
                else
                    throw new Exception("Invalid current property.");
            }

            if ((type & PropertyType.ComplexMask) != 0)
            {
                if (_context.TryGetState(out IComplexSimulationState complex))
                {
                    int nodeIndex = complex.Map[node];
                    if (reference != null)
                    {
                        int referenceIndex = complex.Map[reference];
                        switch (type)
                        {
                            case PropertyType.VoltageReal:
                            case PropertyType.CurrentReal:
                                return () =>
                                {
                                    var solution = complex.Solution;
                                    return solution[nodeIndex].Real - solution[referenceIndex].Real;
                                };
                            case PropertyType.VoltageImaginary:
                            case PropertyType.CurrentImaginary:
                                return () =>
                                {
                                    var solution = complex.Solution;
                                    return solution[nodeIndex].Imaginary - solution[referenceIndex].Imaginary;
                                };
                            case PropertyType.VoltageAmplitude:
                            case PropertyType.CurrentAmplitude:
                                return () =>
                                {
                                    var solution = complex.Solution;
                                    return (solution[nodeIndex] - solution[referenceIndex]).Magnitude;
                                };
                            case PropertyType.VoltageDecibels:
                            case PropertyType.CurrentDecibels:
                                return () =>
                                {
                                    var solution = complex.Solution;
                                    var c = solution[nodeIndex] - solution[referenceIndex];
                                    return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                                };
                            case PropertyType.VoltagePhase:
                            case PropertyType.CurrentPhase:
                                return () =>
                                {
                                    var solution = complex.Solution;
                                    return (solution[nodeIndex] - solution[referenceIndex]).Phase;
                                };
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case PropertyType.VoltageReal:
                            case PropertyType.CurrentReal: return () => complex.Solution[nodeIndex].Real;
                            case PropertyType.VoltageImaginary:
                            case PropertyType.CurrentImaginary: return () => complex.Solution[nodeIndex].Imaginary;
                            case PropertyType.VoltageAmplitude:
                            case PropertyType.CurrentAmplitude: return () => complex.Solution[nodeIndex].Magnitude;
                            case PropertyType.VoltageDecibels:
                            case PropertyType.CurrentDecibels:
                                return () =>
                                {
                                    var c = complex.Solution[nodeIndex];
                                    return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                                };
                            case PropertyType.VoltagePhase:
                            case PropertyType.CurrentPhase: return () => complex.Solution[nodeIndex].Phase;
                        }
                    }
                }
            }
            else
            {
                if (_context.TryGetState(out IBiasingSimulationState biasing))
                {
                    int nodeIndex = biasing.Map[node];
                    if (reference != null)
                    {
                        int referenceIndex = biasing.Map[reference];
                        return () =>
                        {
                            var solution = biasing.Solution;
                            return solution[nodeIndex] - solution[referenceIndex];
                        };
                    }
                    else
                        return () => biasing.Solution[nodeIndex];
                }
            }
            throw new Exception("Unrecognized property");
        }
    }
}
