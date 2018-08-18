using System;
using System.Collections;
using System.Reflection;

namespace Src {

    public static class ReflectionUtil {

        public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        public const BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static Type GetArrayElementTypeOrThrow(Type targetType) {
            bool isListType = typeof(IList).IsAssignableFrom(targetType);

            if (targetType == typeof(IList)) {
                return typeof(object);
            }

            if (!isListType) {
                throw new Exception($"Trying to read the element type of {targetType.Name} but it is not a list type");
            }

            if (targetType.IsArray) {
                return targetType.GetElementType();
            }

            Type[] genericTypes = targetType.GetGenericArguments();
            if (genericTypes.Length == 1) {
                return genericTypes[0];
            }

            throw new Exception($"Trying to read the element type of {targetType.Name} but it is not a list type");

        }

        public static FieldInfo GetFieldInfoOrThrow(Type type, string fieldName) {
            FieldInfo fieldInfo = type.GetField(fieldName, InstanceBindFlags);
            if (fieldInfo == null) {
                throw new Exception($"Field called {fieldName} was not found on type {type.Name}");
            }
            return fieldInfo;
        }

        public static Type GetCommonBaseClass(params Type[] types) {
            if (types.Length == 0) {
                return null;
            }

            Type ret = types[0];

            for (int i = 1; i < types.Length; ++i) {
                if (types[i].IsAssignableFrom(ret))
                    ret = types[i];
                else {
                    // This will always terminate when ret == typeof(object)
                    while (!ret.IsAssignableFrom(types[i]))
                        ret = ret.BaseType;
                }
            }

            return ret;
        }

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
                        case TypeCode.Int32:  return true;
                        case TypeCode.Int64:  return false;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default:              return false;
                    }
                case TypeCode.Single:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte:   return false;
                        case TypeCode.Int16:  return false;
                        case TypeCode.Int32:  return true;
                        case TypeCode.Int64:  return false;
                        case TypeCode.Double: return false;
                        case TypeCode.Single: return true;
                        default:              return false;
                    }
                default:
                    return false;
            }
        }

        public static MethodInfo GetMethodForOperator(Type type, OperatorType opType) {
            // todo what about when the types don't match?
            switch (opType) {
                case OperatorType.Plus:
                    return type.GetMethod("op_Addition", PublicStatic);
            }
            return null;
//            switch (method.Name) {
//                case "op_Implicit":
//                case "op_Explicit":
//                case "op_Addition":
//                case "op_Subtraction":
//                case "op_Multiply":
//                case "op_Division":
//                case "op_Modulus":
//                case "op_ExclusiveOr":
//                case "op_BitwiseAnd":
//                case "op_BitwiseOr":
//                case "op_LogicalAnd":
//                case "op_LogicalOr":
//                case "op_Assign":
//                case "op_LeftShift":
//                case "op_RightShift":
//                case "op_SignedRightShift":
//                case "op_UnsignedRightShift":
//                case "op_Equality":
//                case "op_GreaterThan":
//                case "op_LessThan":
//                case "op_Inequality":
//                case "op_GreaterThanOrEqual":
//                case "op_LessThanOrEqual":
//                case "op_MultiplicationAssignment":
//                case "op_SubtractionAssignment":
//                case "op_ExclusiveOrAssignment":
//                case "op_LeftShiftAssignment":
//                case "op_ModulusAssignment":
//                case "op_AdditionAssignment":
//                case "op_BitwiseAndAssignment":
//                case "op_BitwiseOrAssignment":
//                case "op_Comma":
//                case "op_DivisionAssignment":
//                case "op_Decrement":
//                case "op_Increment":
//                case "op_UnaryNegation":
//                case "op_UnaryPlus":
//                case "op_OnesComplement": break;
//            }
        }

    }

}