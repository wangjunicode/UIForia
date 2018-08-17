using System;

namespace Src {

    public class OperatorExpressionNode : ExpressionNode {

        public ExpressionNode left;
        public ExpressionNode right;
        public IOperatorNode op;

        public OperatorExpressionNode(ExpressionNode right, ExpressionNode left, IOperatorNode op)
            : base(ExpressionNodeType.Operator) {
            this.left = left;
            this.right = right;
            this.op = op;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            Type typeLeft = left.GetYieldedType(context);
            Type typeRight = right.GetYieldedType(context);

            // if(IntTypes(typeLeft, typeRight)) return new ArithmeticOperatorExpression<int>()

            if (op.OpType == OperatorType.Plus) {
                if (typeLeft == typeof(string) || typeRight == typeof(string)) {
                    return typeof(string);
                }
                if (IsNumericType(typeLeft) && IsNumericType(typeRight)) { }
            }

            else if ((OpType & OperatorType.Comparator) != 0) {
                return typeof(bool);
            }
            else if ((OpType & OperatorType.Arithmetic) != 0) {
                return typeLeft;
            }

            throw new Exception("Invalid types");
        }

        public OperatorType OpType => op.OpType;

        public static bool IsNumericType(Type o) {
            switch (Type.GetTypeCode(o)) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

    }

}