using System;

namespace UIForia {

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
            if (yieldedType != null) return yieldedType;
            
            Type typeLeft = left.GetYieldedType(context);
            Type typeRight = right.GetYieldedType(context);

            if ((OpType & OperatorType.Plus) != 0) {
                if (typeLeft == typeof(string) || typeRight == typeof(string)) {
                    yieldedType = typeof(string);
                    return yieldedType;
                }
            }

            if ((OpType & OperatorType.Arithmetic) != 0) {
                if (ReflectionUtil.IsNumericType(typeLeft) && ReflectionUtil.IsNumericType(typeRight)) {

                    if (ReflectionUtil.AreNumericTypesCompatible(typeLeft, typeRight)) {
                        yieldedType = typeLeft;
                        return yieldedType;
                    }

                }
                
                throw new Exception("Invalid types");
            }

            if (OpType == OperatorType.Equals || OpType == OperatorType.NotEquals) {

                if (typeLeft == typeRight) {
                    yieldedType = typeof(bool);
                    return yieldedType;
                }

                if (typeLeft.IsValueType != typeRight.IsValueType) {
                    throw new Exception("Bad comparison");    
                }

                if (typeLeft.IsByRef && typeRight.IsByRef) {
                    yieldedType = typeof(bool);
                    return yieldedType;
                }
                
                if (ReflectionUtil.IsNumericType(typeLeft) && ReflectionUtil.IsNumericType(typeRight)) {
                    yieldedType = typeof(bool);
                    return yieldedType;
                }
             
                return typeof(bool);
            }
            
            if ((OpType & OperatorType.Comparator) != 0) {
                // todo -- typecheck this
                return typeof(bool);
            }

            if ((OpType & OperatorType.Boolean) != 0) {
                if (typeLeft == typeof(bool) && typeRight == typeof(bool)) {
                    return typeof(bool);
                }
            }

            if (OpType == OperatorType.TernaryCondition) {
                return typeof(bool);
            }

            if (OpType == OperatorType.TernarySelection) {
                return typeLeft;
            }
            
            throw new Exception("Invalid types");
        }

        public OperatorType OpType => op.OpType;

        

    }

}