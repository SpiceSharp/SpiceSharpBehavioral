using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// An IL state for real values.
    /// </summary>
    public class RealILState : ILState<double>
    {
        private static readonly MethodInfo _safeDiv = ((Func<double, double, double>)HelperFunctions.SafeDivide).GetMethodInfo();
        private static readonly MethodInfo _power = ((Func<double, double, double>)HelperFunctions.Power).GetMethodInfo();
        private static readonly MethodInfo _equals = ((Func<double, double, double, double, bool>)HelperFunctions.Equals).GetMethodInfo();

        /// <summary>
        /// Creates a new instance of the <see cref="RealILState"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public RealILState(IFunctionBuilder<double> builder)
            : base(builder)
        {
        }

        /// <inheritdoc/>
        public override void Push(Node node)
        {
            Label lblBypass, lblEnd;
            switch (node)
            {
                case BinaryOperatorNode bn:

                    // Execution
                    switch (bn.NodeType)
                    {
                        case NodeTypes.Add:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Add);
                            return;

                        case NodeTypes.Subtract:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Sub);
                            return;

                        case NodeTypes.Multiply:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Mul);
                            return;

                        case NodeTypes.Divide:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Call, _safeDiv);
                            return;

                        case NodeTypes.Modulo:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Rem);
                            return;

                        case NodeTypes.GreaterThan:
                            Push(bn.Left);
                            Push(bn.Right);
                            PushCheck(OpCodes.Bgt, 1.0, 0.0);
                            return;

                        case NodeTypes.LessThan:
                            Push(bn.Left);
                            Push(bn.Right);
                            PushCheck(OpCodes.Blt, 1.0, 0.0);
                            return;

                        case NodeTypes.GreaterThanOrEqual:
                            Push(bn.Left);
                            Push(bn.Right);
                            PushCheck(OpCodes.Bge, 1.0, 0.0);
                            return;

                        case NodeTypes.LessThanOrEqual:
                            Push(bn.Left);
                            Push(bn.Right);
                            PushCheck(OpCodes.Ble, 1.0, 0.0);
                            return;

                        case NodeTypes.Equals:
                            Push(bn.Left);
                            Push(bn.Right);
                            Push(Builder.RelativeTolerance);
                            Push(Builder.AbsoluteTolerance);
                            Generator.Emit(OpCodes.Call, _equals);
                            PushCheck(OpCodes.Brtrue, 1.0, 0.0);
                            return;

                        case NodeTypes.NotEquals:
                            Push(bn.Left);
                            Push(bn.Right);
                            Push(Builder.RelativeTolerance);
                            Push(Builder.AbsoluteTolerance);
                            Generator.Emit(OpCodes.Call, _equals);
                            PushCheck(OpCodes.Brfalse, 1.0, 0.0);
                            return;

                        case NodeTypes.And:
                            lblBypass = Generator.DefineLabel();
                            lblEnd = Generator.DefineLabel();

                            Push(bn.Left); Push(0.5);
                            Generator.Emit(OpCodes.Ble, lblBypass);
                            Push(bn.Right); Push(0.5);
                            Generator.Emit(OpCodes.Ble, lblBypass);
                            Generator.Emit(OpCodes.Ldc_R8, 1.0);
                            Generator.Emit(OpCodes.Br, lblEnd);
                            Generator.MarkLabel(lblBypass);
                            Generator.Emit(OpCodes.Ldc_R8, 0.0);
                            Generator.MarkLabel(lblEnd);
                            return;

                        case NodeTypes.Or:
                            lblBypass = Generator.DefineLabel();
                            lblEnd = Generator.DefineLabel();

                            Push(bn.Left); Push(0.5);
                            Generator.Emit(OpCodes.Bgt, lblBypass);
                            Push(bn.Right); Push(0.5);
                            Generator.Emit(OpCodes.Bgt, lblBypass);
                            Generator.Emit(OpCodes.Ldc_R8, 0.0);
                            Generator.Emit(OpCodes.Br, lblEnd);
                            Generator.MarkLabel(lblBypass);
                            Generator.Emit(OpCodes.Ldc_R8, 1.0);
                            Generator.MarkLabel(lblEnd);
                            return;

                        case NodeTypes.Xor:
                            Push(bn.Left); Push(0.5);
                            Generator.Emit(OpCodes.Cgt);
                            Push(bn.Right); Push(0.5);
                            Generator.Emit(OpCodes.Cgt);
                            Generator.Emit(OpCodes.Xor);
                            PushCheck(OpCodes.Brtrue, 1.0, 0.0);
                            return;

                        case NodeTypes.Pow:
                            Call(HelperFunctions.Power, new[] { bn.Left, bn.Right });
                            return;
                    }
                    break;

                case ConstantNode cn:
                    Push(cn.Literal);
                    return;

                case UnaryOperatorNode un:
                    Push(un.Argument);
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return;
                        case NodeTypes.Minus: Generator.Emit(OpCodes.Neg); return;
                        case NodeTypes.Not:
                            Push(0.5);
                            PushCheck(OpCodes.Ble, 1.0, 0.0);
                            return;
                    }
                    break;

                case FunctionNode fn:
                    var fargs = new FunctionFoundEventArgs<double>(fn, this);
                    OnFunctionFound(fargs);
                    if (!fargs.Created)
                        throw new SpiceSharpException($"Unrecognized function {fn.Name}");
                    return;

                case VariableNode vn:
                    var vargs = new VariableFoundEventArgs<double>(vn);
                    OnVariableFound(vargs);
                    if (vargs.Variable == null)
                        throw new SpiceSharpException($"Unrecognized variable {vn.Name}");
                    var variable = vargs.Variable;
                    Call(() => variable.Value);
                    return;

                case TernaryOperatorNode tn:
                    lblBypass = Generator.DefineLabel();
                    lblEnd = Generator.DefineLabel();
                    Push(tn.Condition); Push(0.5);
                    Generator.Emit(OpCodes.Ble, lblBypass);
                    Push(tn.IfTrue);
                    Generator.Emit(OpCodes.Br, lblEnd);
                    Generator.MarkLabel(lblBypass);
                    Push(tn.IfFalse);
                    Generator.MarkLabel(lblEnd);
                    return;
            }

            throw new Exception("Unrecognized node {0}".FormatString(node));
        }

        /// <inheritdoc/>
        public override void Push(double value)
        {
            Generator.Emit(OpCodes.Ldc_R8, value);
        }
    }
}
