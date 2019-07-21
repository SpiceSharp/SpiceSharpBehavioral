using System;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Operators;
using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Allows parsing an expression using only doubles.
    /// </summary>
    public class SimpleParser : SpiceParser
    {
        /// <summary>
        /// This event is called when a variable is found.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<double>> VariableFound;

        /// <summary>
        /// This event is called when a method call is found.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<double>> FunctionFound;

        /// <summary>
        /// This event is called when a Spice property is found.
        /// </summary>
        public event EventHandler<SpicePropertyFoundEventArgs<double>> SpicePropertyFound;

        /// <summary>
        /// The value stack
        /// </summary>
        private Stack<double> _stack = new Stack<double>();

        /// <summary>
        /// Parse an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public double Parse(string expression)
        {
            _stack.Clear();
            ParseExpression(expression);

            if (_stack.Count != 1)
                throw new ParserException("Invalid expression", Input, Index);
            return _stack.Pop();
        }

        /// <summary>
        /// Execute an operator.
        /// </summary>
        /// <param name="op">The operator.</param>
        protected override void ExecuteOperator(Operator op)
        {
            double a, b;
            switch (op)
            {
                case TernaryOperator to:
                    b = _stack.Pop();
                    a = _stack.Pop();
                    _stack.Push(_stack.Pop().Equals(0.0) ? b : a);
                    break;

                case ClosingTernaryOperator _:
                    break;

                case FunctionOperator fo:

                    // Extract all arguments for the method
                    var arguments = new double[fo.Arguments];
                    for (var i = fo.Arguments - 1; i >= 0; i--)
                        arguments[i] = _stack.Pop();
                    var args = new FunctionFoundEventArgs<double>(fo.Name, double.NaN, arguments);
                    
                    // Ask around for the result of this method
                    FunctionFound?.Invoke(this, args);
                    if (double.IsNaN(args.Result))
                        throw new ParserException("Unrecognized method '{0}()'".FormatString(fo.Name), Input, Index);
                    _stack.Push(args.Result);
                    break;

                case BracketOperator _:
                    break;
                case ArgumentOperator _:
                    break;

                case ArithmeticOperator ao:
                    switch (ao.Type)
                    {
                        case OperatorType.Positive:
                            break;
                        case OperatorType.Negative:
                            _stack.Push(-_stack.Pop());
                            break;
                        case OperatorType.Not:
                            _stack.Push(_stack.Pop().Equals(0.0) ? 0.0 : 1.0);
                            break;
                        case OperatorType.Add:
                            _stack.Push(_stack.Pop() + _stack.Pop());
                            break;
                        case OperatorType.Subtract:
                            a = _stack.Pop();
                            _stack.Push(_stack.Pop() - a);
                            break;
                        case OperatorType.Multiply:
                            _stack.Push(_stack.Pop() * _stack.Pop());
                            break;
                        case OperatorType.Divide:
                            a = _stack.Pop();
                            _stack.Push(_stack.Pop() / a);
                            break;
                        case OperatorType.Modulo:
                            a = _stack.Pop();
                            _stack.Push(_stack.Pop() % a);
                            break;
                        case OperatorType.Power:
                            a = _stack.Pop();
                            _stack.Push(Math.Pow(_stack.Pop(), a));
                            break;
                        case OperatorType.ConditionalOr:
                            b = _stack.Pop();
                            a = _stack.Pop();
                            _stack.Push(a.Equals(0.0) && b.Equals(0.0) ? 0.0 : 1.0);
                            break;
                        case OperatorType.ConditionalAnd:
                            b = _stack.Pop();
                            a = _stack.Pop();
                            _stack.Push(a.Equals(0.0) || b.Equals(0.0) ? 0.0 : 1.0);
                            break;
                        case OperatorType.GreaterThan:
                            b = _stack.Pop();
                            a = _stack.Pop();
                            _stack.Push(a > b ? 1.0 : 0.0);
                            break;
                        case OperatorType.LessThan:
                            b = _stack.Pop();
                            a = _stack.Pop();
                            _stack.Push(a < b ? 1.0 : 0.0);
                            break;
                        case OperatorType.GreaterThanOrEqual:
                            b = _stack.Pop();
                            a = _stack.Pop();
                            _stack.Push(a >= b ? 1.0 : 0.0);
                            break;
                        case OperatorType.LessThanOrEqual:
                            b = _stack.Pop();
                            a = _stack.Pop();
                            _stack.Push(a <= b ? 1.0 : 0.0);
                            break;
                        default:
                            throw new ParserException("Unimplemented arithmetic operator", Input, Index);
                    }
                    break;

                default:
                    throw new ParserException("Unimplemented operator", Input, Index);
            }
        }

        /// <summary>
        /// Push a double value.
        /// </summary>
        /// <param name="value">The value.</param>
        protected override void PushValue(double value)
        {
            _stack.Push(value);
        }

        /// <summary>
        /// Push a Spice property value.
        /// </summary>
        /// <param name="property">The property.</param>
        protected override void PushSpiceProperty(SpiceProperty property)
        {
            var args = new SpicePropertyFoundEventArgs<double>(property, double.NaN);
            SpicePropertyFound?.Invoke(this, args);

            if (double.IsNaN(args.Result))
                throw new ParserException("Unrecognized Spice property '{0}'".FormatString(property), Input, Index);
            _stack.Push(args.Result);
        }

        /// <summary>
        /// Push a variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        protected override void PushVariable(string name)
        {
            var args = new VariableFoundEventArgs<double>(name, double.NaN);
            VariableFound?.Invoke(this, args);

            if (double.IsNaN(args.Result))
                throw new ParserException("Unrecognized variable '{0}'".FormatString(name), Input, Index);
            _stack.Push(args.Result);
        }
    }
}
