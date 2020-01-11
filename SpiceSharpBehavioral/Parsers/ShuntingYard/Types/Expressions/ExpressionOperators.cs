using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Defines double arithmetic through expression trees that can be compiled.
    /// </summary>
    /// <seealso cref="IShuntingYardOperators{T}" />
    public class ExpressionOperators : IShuntingYardOperators<Expression>
    {
        private readonly Expression _zero = Expression.Constant(0.0);
        private readonly Expression _one = Expression.Constant(1.0);

        /// <summary>
        /// Occurs when a variable was found.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<Expression>> VariableFound;

        /// <summary>
        /// Occurs when a function was found.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<Expression>> FunctionFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionOperators"/> class.
        /// </summary>
        public ExpressionOperators()
        {
            VariableFound += ExpressionDefaults.VariableFound;
            FunctionFound += ExpressionDefaults.FunctionFound;
        }

        /// <summary>
        /// Addition.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Add(Expression left, Expression right) => Expression.Add(left, right);

        /// <summary>
        /// And.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression And(Expression left, Expression right)
        {
            return Expression.Condition(
                Expression.OrElse(
                    Expression.Equal(left, _zero),
                    Expression.Equal(right, _zero)),
                _zero, _one);
        }

        /// <summary>
        /// Creates a function value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The function value.
        /// </returns>
        /// <exception cref="ParserException">Thrown if no function was found.</exception>
        public Expression CreateFunction(string name, IReadOnlyList<Expression> arguments)
        {
            var args = new FunctionFoundEventArgs<Expression>(name, arguments);
            FunctionFound?.Invoke(this, args);
            if (args.Found)
            {
                if (args.Result.Type != typeof(double))
                    throw new ParserException("Function '{0}' does not return a double".FormatString(name));
                return args.Result;
            }
            throw new ParserException("Unrecognized function '{0}'".FormatString(name));
        }

        /// <summary>
        /// Creates a value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public Expression CreateValue(double input, string unit = "") => Expression.Constant(input);

        /// <summary>
        /// Creates a variable value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="ParserException">Thrown if no variable was found.</exception>
        public Expression CreateVariable(string name)
        {
            var args = new VariableFoundEventArgs<Expression>(name);
            VariableFound?.Invoke(this, args);
            if (args.Found)
            {
                if (args.Result.Type != typeof(double))
                    throw new ParserException("Variable '{0}' is not a double".FormatString(name));
                return args.Result;
            }
            throw new ParserException("Unrecognized variable '{0}'".FormatString(name));
        }

        /// <summary>
        /// Division.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Divide(Expression left, Expression right) => Expression.Divide(left, right);

        /// <summary>
        /// Equality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Equal(Expression left, Expression right)
            => Expression.Condition(Expression.Equal(left, right), _one, _zero);

        /// <summary>
        /// Greater than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Greater(Expression left, Expression right)
            => Expression.Condition(Expression.GreaterThan(left, right), _one, _zero);

        /// <summary>
        /// Greater or equal than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression GreaterOrEqual(Expression left, Expression right)
            => Expression.Condition(Expression.GreaterThanOrEqual(left, right), _one, _zero);

        /// <summary>
        /// A conditional.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression IfThenElse(Expression condition, Expression ifTrue, Expression ifFalse)
            => Expression.Condition(Expression.Equal(condition, _zero), ifFalse, ifTrue);

        /// <summary>
        /// Less than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Less(Expression left, Expression right)
            => Expression.Condition(Expression.LessThan(left, right), _one, _zero);

        /// <summary>
        /// Less or equal than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression LessOrEqual(Expression left, Expression right)
            => Expression.Condition(Expression.LessThanOrEqual(left, right), _one, _zero);

        /// <summary>
        /// Remainder.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Modulo(Expression left, Expression right) => Expression.Modulo(left, right);

        /// <summary>
        /// Multiplication.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Multiply(Expression left, Expression right) => Expression.Multiply(left, right);

        /// <summary>
        /// Applies a unary not operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Not(Expression argument)
            => Expression.Condition(Expression.Equal(argument, _zero), _one, _zero);

        /// <summary>
        /// Inequality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression NotEqual(Expression left, Expression right)
            => Expression.Condition(Expression.NotEqual(left, right), _one, _zero);

        /// <summary>
        /// Or.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Or(Expression left, Expression right)
        {
            return Expression.Condition(
                Expression.AndAlso(
                    Expression.Equal(left, _zero),
                    Expression.Equal(right, _zero)),
                _zero,
                _one);
        }

        /// <summary>
        /// Raising to a power.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Pow(Expression left, Expression right) => Expression.Power(left, right);

        /// <summary>
        /// Logarithm.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// Rhe result.
        /// </returns>
        public Expression Log(Expression argument) => Expression.Call(null, ((Func<double, double>)Math.Log).GetMethodInfo(), argument);

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Subtract(Expression left, Expression right) => Expression.Subtract(left, right);

        /// <summary>
        /// Applies a unary minus operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression UnaryMinus(Expression argument) => Expression.Negate(argument);

        /// <summary>
        /// Applies a unary plus operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression UnaryPlus(Expression argument) => argument;

        /// <summary>
        /// Sign of the argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Expression Sign(Expression argument) => Expression.Convert(Expression.Call(null, ((Func<double, int>)Math.Sign).GetMethodInfo(), argument), typeof(double));
    }
}
