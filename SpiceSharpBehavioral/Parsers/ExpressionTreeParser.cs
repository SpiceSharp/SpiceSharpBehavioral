using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SpiceSharpBehavioral.Parsers.Operators;
using SpiceSharp;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Allows parsing expressions into a method. The method signature should
    /// match the expression.
    /// </summary>
    public class ExpressionTreeParser : SpiceParser, IParser<Expression>
    {
        private static readonly MethodInfo PowInfo = typeof(Math).GetTypeInfo().GetMethod("Pow", new Type[] { typeof(double), typeof(double) });
        private static readonly Expression Zero = Expression.Constant(0.0);
        private static readonly Expression One = Expression.Constant(1.0);

        /// <summary>
        /// This event is called when a variable has been encountered.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<Expression>> VariableFound;

        /// <summary>
        /// This event is called when a function call has been encountered.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<Expression>> FunctionFound;

        /// <summary>
        /// this event is called when a Spice property has been encountered.
        /// </summary>
        public event EventHandler<SpicePropertyFoundEventArgs<double>> SpicePropertyFound;

        /// <summary>
        /// The value stack.
        /// </summary>
        Stack<Expression> _stack = new Stack<Expression>();

        /// <summary>
        /// Parse an expression string into a Linq expression tree.
        /// </summary>
        /// <param name="expression">The expression string.</param>
        /// <returns>The Linq expression tree.</returns>
        public Expression Parse(string expression)
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
            Expression a, b;
            switch (op)
            {
                case TernaryOperator to:
                    b = _stack.Pop();
                    a = _stack.Pop();
                    _stack.Push(Expression.Condition(_stack.Pop(), a, b));
                    break;

                case ClosingTernaryOperator _:
                    break;

                case FunctionOperator fo:

                    // Extract all arguments for the method
                    var arguments = new Expression[fo.Arguments];
                    for (var i = fo.Arguments - 1; i >= 0; i--)
                        arguments[i] = _stack.Pop();
                    var args = new FunctionFoundEventArgs<Expression>(fo.Name, null, arguments);

                    // Ask around for the result of this method
                    FunctionFound?.Invoke(this, args);
                    if (args.Result == null)
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
                            _stack.Push(Expression.Negate(_stack.Pop()));
                            break;
                        case OperatorType.Not:
                            _stack.Push(Expression.Condition(Expression.Equal(_stack.Pop(), Zero), Zero, One));
                            break;
                        case OperatorType.Add:
                            a = _stack.Pop();
                            _stack.Push(Expression.Add(_stack.Pop(), a));
                            break;
                        case OperatorType.Subtract:
                            a = _stack.Pop();
                            _stack.Push(Expression.Subtract(_stack.Pop(), a));
                            break;
                        case OperatorType.Multiply:
                            _stack.Push(Expression.Multiply(_stack.Pop(), _stack.Pop()));
                            break;
                        case OperatorType.Divide:
                            a = _stack.Pop();
                            _stack.Push(Expression.Divide(_stack.Pop(), a));
                            break;
                        case OperatorType.Modulo:
                            a = _stack.Pop();
                            _stack.Push(Expression.Modulo(_stack.Pop(), a));
                            break;
                        case OperatorType.Power:
                            a = _stack.Pop();
                            _stack.Push(Expression.Call(PowInfo, _stack.Pop(), a));
                            break;
                        case OperatorType.ConditionalOr:
                            b = _stack.Pop();
                            a = _stack.Pop();
                            _stack.Push(Expression.Condition(Expression.And(
                                Expression.Equal(a, Zero),
                                Expression.Equal(b, Zero)), Zero, One));
                            break;
                        case OperatorType.ConditionalAnd:
                            b = _stack.Pop();
                            a = _stack.Pop();
                            _stack.Push(Expression.Condition(Expression.Or(
                                Expression.Equal(a, Zero),
                                Expression.Equal(b, Zero)), Zero, One));
                            break;
                        case OperatorType.GreaterThan:
                            a = _stack.Pop();
                            _stack.Push(Expression.GreaterThan(_stack.Pop(), a));
                            break;
                        case OperatorType.LessThan:
                            a = _stack.Pop();
                            _stack.Push(Expression.LessThan(_stack.Pop(), a));
                            break;
                        case OperatorType.GreaterThanOrEqual:
                            a = _stack.Pop();
                            _stack.Push(Expression.GreaterThanOrEqual(_stack.Pop(), a));
                            break;
                        case OperatorType.LessThanOrEqual:
                            a = _stack.Pop();
                            _stack.Push(Expression.LessThanOrEqual(_stack.Pop(), a));
                            break;
                        case OperatorType.OpenTernary:

                        default:
                            throw new ParserException("Unimplemented arithmetic operator", Input, Index);
                    }
                    break;

                default:
                    throw new ParserException("Unimplemented operator", Input, Index);
            }
        }

        /// <summary>
        /// Pushes a value on the value stack.
        /// </summary>
        /// <param name="value">The value.</param>
        protected override void PushValue(double value)
        {
            _stack.Push(Expression.Constant(value));
        }

        /// <summary>
        /// Push a spice property on the value stack.
        /// </summary>
        /// <param name="property">The Spice property.</param>
        protected override void PushSpiceProperty(SpiceProperty property)
        {
            var args = new ExpressionTreeSpicePropertyEventArgs(property);
            SpicePropertyFound?.Invoke(this, args);

            if (args.Result == null)
                throw new ParserException("Unrecognized Spice property '{0}'".FormatString(property), Input, Index);
            _stack.Push(args.Result);
        }

        /// <summary>
        /// Pushes a variable value on the value stack.
        /// </summary>
        /// <param name="name">The variable name.</param>
        protected override void PushVariable(string name)
        {
            var args = new VariableFoundEventArgs<Expression>(name, null);
            VariableFound?.Invoke(this, args);

            if (args.Result == null)
                throw new ParserException("Unrecognized variable '{0}'".FormatString(name), Input, Index);
            _stack.Push(args.Result);
        }
    }
}
