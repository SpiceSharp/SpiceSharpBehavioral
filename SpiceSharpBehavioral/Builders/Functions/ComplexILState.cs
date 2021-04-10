using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// An IL state for complex values.
    /// </summary>
    public class ComplexILState : ILState<Complex>, IILComplexState
    {
        private static readonly MethodInfo _safeDiv = ((Func<Complex, Complex, Complex>)HelperFunctions.SafeDivide).GetMethodInfo();
        private static readonly MethodInfo _equals = ((Func<Complex, Complex, double, double, bool>)HelperFunctions.Equals).GetMethodInfo();
        private static readonly ConstructorInfo _cplx = typeof(Complex).GetTypeInfo().GetConstructor(new[] { typeof(double), typeof(double) });
        private static readonly MethodInfo _realPart = typeof(Complex).GetTypeInfo().GetProperty("Real", typeof(double)).GetGetMethod();

        /// <summary>
        /// Creates a new instance of the <see cref="ComplexILState"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public ComplexILState(IFunctionBuilder<Complex> builder)
            : base(builder)
        {
            // Let's reserve some space for calling properties
            Generator.DeclareLocal(typeof(Complex));
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
                            Call(Complex.Add, new[] { bn.Left, bn.Right });
                            return;

                        case NodeTypes.Subtract:
                            Call(Complex.Subtract, new[] { bn.Left, bn.Right });
                            return;

                        case NodeTypes.Multiply:
                            Call(Complex.Multiply, new[] { bn.Left, bn.Right });
                            return;

                        case NodeTypes.Divide:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Call, _safeDiv);
                            return;

                        case NodeTypes.Modulo:
                            PushReal(bn.Left);
                            PushReal(bn.Right);
                            Generator.Emit(OpCodes.Rem);
                            RealToComplex();
                            return;

                        case NodeTypes.GreaterThan:
                            PushReal(bn.Left);
                            PushReal(bn.Right);
                            PushCheck(OpCodes.Bgt_S, 1.0, 0.0);
                            return;

                        case NodeTypes.LessThan:
                            PushReal(bn.Left);
                            PushReal(bn.Right);
                            PushCheck(OpCodes.Blt_S, 1.0, 0.0);
                            return;

                        case NodeTypes.GreaterThanOrEqual:
                            PushReal(bn.Left);
                            PushReal(bn.Right);
                            PushCheck(OpCodes.Bge_S, 1.0, 0.0);
                            return;

                        case NodeTypes.LessThanOrEqual:
                            PushReal(bn.Left);
                            PushReal(bn.Right);
                            PushCheck(OpCodes.Ble_S, 1.0, 0.0);
                            return;

                        case NodeTypes.Equals:
                            Push(bn.Left);
                            Push(bn.Right);
                            PushDouble(Builder.RelativeTolerance);
                            PushDouble(Builder.AbsoluteTolerance);
                            Generator.Emit(OpCodes.Call, _equals);
                            PushCheck(OpCodes.Brtrue_S, 1.0, 0.0);
                            return;

                        case NodeTypes.NotEquals:
                            Push(bn.Left);
                            Push(bn.Right);
                            PushDouble(Builder.RelativeTolerance);
                            PushDouble(Builder.AbsoluteTolerance);
                            Generator.Emit(OpCodes.Call, _equals);
                            PushCheck(OpCodes.Brfalse_S, 1.0, 0.0);
                            return;

                        case NodeTypes.And:
                            lblBypass = Generator.DefineLabel();
                            lblEnd = Generator.DefineLabel();

                            PushReal(bn.Left); PushDouble(0.5);
                            Generator.Emit(OpCodes.Ble_S, lblBypass);
                            PushReal(bn.Right); PushDouble(0.5);
                            Generator.Emit(OpCodes.Ble_S, lblBypass);
                            Push(new Complex(1.0, 0.0));
                            Generator.Emit(OpCodes.Br_S, lblEnd);
                            Generator.MarkLabel(lblBypass);
                            Push(new Complex());
                            Generator.MarkLabel(lblEnd);
                            return;

                        case NodeTypes.Or:
                            lblBypass = Generator.DefineLabel();
                            lblEnd = Generator.DefineLabel();

                            PushReal(bn.Left); PushDouble(0.5);
                            Generator.Emit(OpCodes.Bgt_S, lblBypass);
                            PushReal(bn.Right); PushDouble(0.5);
                            Generator.Emit(OpCodes.Bgt_S, lblBypass);
                            Push(new Complex());
                            Generator.Emit(OpCodes.Br_S, lblEnd);
                            Generator.MarkLabel(lblBypass);
                            Push(new Complex(1, 0));
                            Generator.MarkLabel(lblEnd);
                            return;

                        case NodeTypes.Xor:
                            PushReal(bn.Left); PushDouble(0.5);
                            Generator.Emit(OpCodes.Cgt);
                            PushReal(bn.Right); PushDouble(0.5);
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
                    Push(new Complex(cn.Literal, 0.0));
                    return;

                case UnaryOperatorNode un:
                    
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus:
                            Push(un.Argument);
                            return;
                        case NodeTypes.Minus:
                            Call(Complex.Negate, new[] { un.Argument });
                            return;

                        case NodeTypes.Not:
                            PushReal(un.Argument);
                            PushDouble(0.5);
                            PushCheck(OpCodes.Ble_S, 1.0, 0.0);
                            return;
                    }
                    break;

                case FunctionNode fn:
                    var fnargs = new FunctionFoundEventArgs<Complex>(fn, this);
                    OnFunctionFound(fnargs);
                    if (!fnargs.Created)
                        throw new SpiceSharpException($"Unrecognized function {fn.Name}");
                    return;

                case VariableNode vn:
                    var varargs = new VariableFoundEventArgs<Complex>(vn);
                    OnVariableFound(varargs);
                    if (varargs.Variable == null)
                        throw new SpiceSharpException($"Unrecognized variable {vn.Name}");
                    var variable = varargs.Variable;
                    Call(() => variable.Value);
                    return;

                case TernaryOperatorNode tn:
                    lblBypass = Generator.DefineLabel();
                    lblEnd = Generator.DefineLabel();
                    PushReal(tn.Condition); PushDouble(0.5);
                    Generator.Emit(OpCodes.Ble_S, lblBypass);
                    Push(tn.IfTrue);
                    Generator.Emit(OpCodes.Br_S, lblEnd);
                    Generator.MarkLabel(lblBypass);
                    Push(tn.IfFalse);
                    Generator.MarkLabel(lblEnd);
                    return;
            }

            throw new Exception("Unrecognized node {0}".FormatString(node));
        }

        /// <inheritdoc/>
        public override void Push(Complex value)
        {
            Generator.Emit(OpCodes.Ldc_R8, value.Real);
            Generator.Emit(OpCodes.Ldc_R8, value.Imaginary);
            Generator.Emit(OpCodes.Newobj, _cplx);
        }

        /// <inheritdoc/>
        public void PushReal(Node node = null)
        {
            if (node != null)
                Push(node);
            Generator.Emit(OpCodes.Stloc_0);
            Generator.Emit(OpCodes.Ldloca_S, 0);
            Generator.Emit(OpCodes.Call, _realPart);
        }

        /// <inheritdoc/>
        public void RealToComplex()
        {
            PushDouble(0.0);
            Generator.Emit(OpCodes.Newobj, _cplx);
        }
    }
}
