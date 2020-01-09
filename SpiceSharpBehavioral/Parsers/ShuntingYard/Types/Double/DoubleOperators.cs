using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Defines default double operators.
    /// </summary>
    /// <seealso cref="IShuntingYardOperators{T}" />
    public class DoubleOperators : IShuntingYardOperators<double>
    {
        /// <summary>
        /// Occurs when a variable was found.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<double>> VariableFound;

        /// <summary>
        /// Occurs when a function was found.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<double>> FunctionFound;

        /// <summary>
        /// Addition.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Add(double left, double right) => left + right;

        /// <summary>
        /// Creates a function value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The function value.
        /// </returns>
        /// <exception cref="ParserException">Thrown if no function was found.</exception>
        public double CreateFunction(string name, IReadOnlyList<double> arguments)
        {
            var args = new FunctionFoundEventArgs<double>(name, arguments);
            FunctionFound?.Invoke(this, args);
            if (args.Found)
                return args.Result;
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
        public double CreateValue(double input, string unit = "") => input;

        /// <summary>
        /// Creates a variable value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        /// <exception cref="ParserException">Thrown if no variable was found.</exception>
        public double CreateVariable(string name)
        {
            var args = new VariableFoundEventArgs<double>(name);
            VariableFound?.Invoke(this, args);
            if (args.Found)
                return args.Result;
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
        public double Divide(double left, double right) => left / right;

        /// <summary>
        /// Equality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Equal(double left, double right) => left.Equals(right) ? 1.0 : 0.0;

        /// <summary>
        /// Greater than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Greater(double left, double right) => left > right ? 1.0 : 0.0;

        /// <summary>
        /// Greater or equal than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double GreaterOrEqual(double left, double right) => left >= right ? 1.0 : 0.0;

        /// <summary>
        /// A conditional.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double IfThenElse(double condition, double ifTrue, double ifFalse) => condition.Equals(0.0) ? ifFalse : ifTrue;

        /// <summary>
        /// Less than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Less(double left, double right) => left < right ? 1.0 : 0.0;

        /// <summary>
        /// Less or equal than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double LessOrEqual(double left, double right) => left <= right ? 1.0 : 0.0;

        /// <summary>
        /// Remainder.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Modulo(double left, double right) => left % right;

        /// <summary>
        /// Multiplication.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Multiply(double left, double right) => left * right;

        /// <summary>
        /// Applies a unary minus operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double UnaryMinus(double argument) => -argument;

        /// <summary>
        /// Applies a unary not operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Not(double argument) => argument.Equals(0.0) ? 1 : 0;

        /// <summary>
        /// Inequality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double NotEqual(double left, double right) => left.Equals(right) ? 0 : 1;

        /// <summary>
        /// Applies a unary plus operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double UnaryPlus(double argument) => argument;

        /// <summary>
        /// Raising to a power.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Pow(double left, double right) => Math.Pow(left, right);

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Subtract(double left, double right) => left - right;

        /// <summary>
        /// And.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double And(double left, double right) => left.Equals(0.0) || right.Equals(0.0) ? 0 : 1;

        /// <summary>
        /// Or.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public double Or(double left, double right) => left.Equals(0.0) && right.Equals(0.0) ? 0 : 1;
    }
}
