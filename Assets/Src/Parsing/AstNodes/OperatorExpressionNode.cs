using System;
using UnityEngine;

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

            if ((OpType & OperatorType.Plus) != 0) {
                if (typeLeft == typeof(string) || typeRight == typeof(string)) {
                    return typeof(string);
                }
            }

            if ((OpType & OperatorType.Arithmetic) != 0) {
                if (IsNumericType(typeLeft) && IsNumericType(typeRight)) {

                    if (AreNumericTypesCompatible(typeLeft, typeRight)) {
                        return typeLeft;
                    }

                }
                
                throw new Exception("Invalid types");
            }

            if (OpType == OperatorType.Equals || OpType == OperatorType.NotEquals) {
                // both numbers
                // both reference types assignable to eachother
                // both value types assignable to eachother
                // if same type or derivitive type can use == w/ cast
                // if not need to use .Equals
                // comparing a reference type to a value type is invalid
                // todo -- type chekc this
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
                throw new Exception("Invalid types");
            }
         
            throw new Exception("Invalid types");
        }

        // todo -- might need to reverse left & right in switches
        public static bool AreNumericTypesCompatible(Type left, Type right) {
            switch (Type.GetTypeCode(left)) {
                case TypeCode.Byte:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte:   return true;
                        case TypeCode.Int16:  return true;
                        case TypeCode.Int32:  return true;
                        case TypeCode.Int64:  return true;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default:              return false;
                    }
                case TypeCode.Int16:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte:   return false;
                        case TypeCode.Int16:  return true;
                        case TypeCode.Int32:  return true;
                        case TypeCode.Int64:  return true;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default:              return false;
                    }
                case TypeCode.Int32:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte:   return false;
                        case TypeCode.Int16:  return false;
                        case TypeCode.Int32:  return true;
                        case TypeCode.Int64:  return true;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default:              return false;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte:   return false;
                        case TypeCode.Int16:  return false;
                        case TypeCode.Int32:  return false;
                        case TypeCode.Int64:  return true;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default:              return false;
                    }
                case TypeCode.Double:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte:   return false;
                        case TypeCode.Int16:  return false;
                        case TypeCode.Int32:  return false;
                        case TypeCode.Int64:  return false;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return false;
                        default:              return false;
                    }
                case TypeCode.Single:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte:   return false;
                        case TypeCode.Int16:  return false;
                        case TypeCode.Int32:  return false;
                        case TypeCode.Int64:  return false;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default:              return false;
                    }
                default:
                    return false;
            }
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