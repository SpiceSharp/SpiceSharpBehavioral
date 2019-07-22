using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Operators;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers
{
    public class ExpressionTreeDerivativeParser : SpiceParser
    {
        private static readonly MethodInfo PowInfo = typeof(Math).GetTypeInfo().GetMethod("Pow", new Type[] { typeof(double), typeof(double) });
        private static readonly MethodInfo LogInfo = typeof(Math).GetTypeInfo().GetMethod("Log", new Type[] { typeof(double) });
        private static readonly Expression Zero = Expression.Constant(0.0);
        private static readonly Expression One = Expression.Constant(1.0);

        /// <summary>
        /// This event is called when a variable is found.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<ExpressionTreeDerivatives>> VariableFound;

        /// <summary>
        /// This event is called when a method call is found.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<ExpressionTreeDerivatives>> FunctionFound;

        /// <summary>
        /// This event is called when a Spice property is found.
        /// </summary>
        public event EventHandler<SpicePropertyFoundEventArgs<ExpressionTreeDerivatives>> SpicePropertyFound;

        /// <summary>
        /// The value stack
        /// </summary>
        private Stack<ExpressionTreeDerivatives> _stack = new Stack<ExpressionTreeDerivatives>();

        /// <summary>
        /// Parse an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public ExpressionTreeDerivatives Parse(string expression)
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
            ExpressionTreeDerivatives a, b;
            switch (op)
            {
                case TernaryOperator to:
                    b = _stack.Pop();
                    a = _stack.Pop();
                    _stack.Push(_stack.Pop().IfThen(a, b));
                    break;

                case ClosingTernaryOperator _:
                    break;

                case FunctionOperator fo:

                    // Extract all arguments for the method
                    var arguments = new ExpressionTreeDerivatives[fo.Arguments];
                    for (var i = fo.Arguments - 1; i >= 0; i--)
                        arguments[i] = _stack.Pop();
                    var args = new FunctionFoundEventArgs<ExpressionTreeDerivatives>(fo.Name, null, arguments);

                    // Ask around for the result of this method
                    FunctionFound?.Invoke(this, args);
                    if (!args.Found || args.Result == null)
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
                            _stack.Push(!_stack.Pop());
                            break;
                        case OperatorType.Add:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop() + b);
                            break;
                        case OperatorType.Subtract:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop() - b);
                            break;
                        case OperatorType.Multiply:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop() * b);
                            break;
                        case OperatorType.Divide:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop() / b);
                            break;
                        case OperatorType.Power:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop().Pow(b));
                            break;
                        case OperatorType.ConditionalOr:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop().Or(b));
                            break;
                        case OperatorType.ConditionalAnd:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop().And(b));
                            break;
                        case OperatorType.GreaterThan:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop() > b);
                            break;
                        case OperatorType.LessThan:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop() < b);
                            break;
                        case OperatorType.GreaterThanOrEqual:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop() >= b);
                            break;
                        case OperatorType.LessThanOrEqual:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop() <= b);
                            break;
                        case OperatorType.IsEqual:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop().Equal(b));
                            break;
                        case OperatorType.IsNotEqual:
                            b = _stack.Pop();
                            _stack.Push(_stack.Pop().NotEqual(b));
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
            var d = new ExpressionTreeDerivatives();
            d[0] = Expression.Constant(value);
            _stack.Push(d);
        }

        /// <summary>
        /// Push a Spice property value.
        /// </summary>
        /// <param name="property">The property.</param>
        protected override void PushSpiceProperty(SpiceProperty property)
        {
            var args = new SpicePropertyFoundEventArgs<ExpressionTreeDerivatives>(property, null);
            SpicePropertyFound?.Invoke(this, args);

            if (!args.Found || args.Result == null)
                throw new ParserException("Unrecognized Spice property '{0}'".FormatString(property), Input, Index);
            _stack.Push(args.Result);
        }

        /// <summary>
        /// Push a variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        protected override void PushVariable(string name)
        {
            var args = new VariableFoundEventArgs<ExpressionTreeDerivatives>(name, null);
            VariableFound?.Invoke(this, args);

            if (!args.Found || args.Result == null)
                throw new ParserException("Unrecognized variable '{0}'".FormatString(name), Input, Index);
            _stack.Push(args.Result);
        }
    }
}
