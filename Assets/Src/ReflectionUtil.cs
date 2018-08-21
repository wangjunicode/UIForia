using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Src;
using Expression = System.Linq.Expressions.Expression;

public static class ReflectionUtil {

    private struct GenericTypeEntry {

        public readonly Type[] paramTypes;
        public readonly Type retnType;
        public readonly Type baseType;

        public GenericTypeEntry(Type baseType, Type[] paramTypes, Type retnType) {
            this.baseType = baseType;
            this.paramTypes = paramTypes;
            this.retnType = retnType;
        }

    }

    private struct DelegateEntry {

        public readonly Delegate instance;
        public readonly MethodInfo methodInfo;

        public DelegateEntry(Delegate instance, MethodInfo methodInfo) {
            this.instance = instance;
            this.methodInfo = methodInfo;
        }

    }

    public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
    public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
    public const BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    public const BindingFlags InterfaceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

    private static readonly List<GenericTypeEntry> generics = new List<GenericTypeEntry>();
    private static readonly List<DelegateEntry> staticDelegates = new List<DelegateEntry>();
    private static readonly List<DelegateEntry> openDelegates = new List<DelegateEntry>();
    
    private static Dictionary<Type, List<LinqAccessor>> linqDelegates = new Dictionary<Type, List<LinqAccessor>>();

    public static readonly object[] ObjectArray0 = new object[0];
    public static readonly object[] ObjectArray1 = new object[1];
    public static readonly object[] ObjectArray2 = new object[2];
    public static readonly object[] ObjectArray3 = new object[3];

    public static readonly Type[] TypeArray1 = new Type[1];
    public static readonly Type[] TypeArray2 = new Type[2];
    public static readonly Type[] TypeArray3 = new Type[3];

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

    public static object CreateGenericInstance(Type genericBase, params object[] args) {
        return Activator.CreateInstance(genericBase, args);
    }

    public static object CreateGenericInstanceFromOpenType(Type openBaseType, Type genericArgument, params object[] args) {
        Type genericType = openBaseType.MakeGenericType(genericArgument);
        return Activator.CreateInstance(genericType, args);
    }

    public static object CreateGenericInstanceFromOpenType(Type openBaseType, Type[] genericArguments, params object[] args) {
        Type genericType = openBaseType.MakeGenericType(genericArguments);
        return Activator.CreateInstance(genericType, args);
    }

    public static Type CreateGenericType(Type baseType, params Type[] genericArguments) {
        for (int i = 0; i < generics.Count; i++) {
            GenericTypeEntry entry = generics[i];
            if (entry.baseType != baseType || genericArguments.Length != entry.paramTypes.Length) {
                continue;
            }

            if (!TypeParamsMatch(entry.paramTypes, genericArguments)) {
                continue;
            }

            return entry.retnType;
        }

        Type outputType = baseType.MakeGenericType(genericArguments);
        GenericTypeEntry newType = new GenericTypeEntry(
            baseType,
            genericArguments,
            outputType
        );
        generics.Add(newType);
        return outputType;
    }

    private static bool TypeParamsMatch(Type[] params0, Type[] params1) {
        if (params0.Length != params1.Length) return false;
        for (int i = 0; i < params0.Length; i++) {
            if (params0[i] != params1[i]) {
                return false;
            }
        }

        return true;
    }

    public static bool HasAnyAttribute(MethodInfo methodInfo, params Type[] types) {
        for (int i = 0; i < types.Length; i++) {
            if (methodInfo.GetCustomAttribute(types[i]) != null) {
                return true;
            }
        }

        return false;
    }

    public static bool HasAnyAttribute(FieldInfo fieldInfo, params Type[] types) {
        for (int i = 0; i < types.Length; i++) {
            if (fieldInfo.GetCustomAttribute(types[i]) != null) {
                return true;
            }
        }

        return false;
    }

    public static TDelegateType CreateOpenDelegate<TDelegateType>(MethodInfo info) where TDelegateType : class {
        return Delegate.CreateDelegate(typeof(TDelegateType), null, info) as TDelegateType;
    }

    public static Delegate CreateOpenDelegate(Type type, MethodInfo info) {
        return Delegate.CreateDelegate(type, null, info);
    }

    public static Type GetOpenDelegateType(MethodInfo info) {
        ParameterInfo[] parameters = info.GetParameters();
        Type[] signatureTypes = new Type[parameters.Length + 2];

        signatureTypes[0] = info.DeclaringType;

        for (int i = 1; i < parameters.Length + 1; i++) {
            signatureTypes[i] = parameters[i - 1].ParameterType;
        }

        signatureTypes[parameters.Length + 1] = info.ReturnType;

        switch (signatureTypes.Length) {
            case 1:
                return typeof(Func<>).MakeGenericType(signatureTypes);
            case 2:
                return typeof(Func<,>).MakeGenericType(signatureTypes);
            case 3:
                return typeof(Func<,,>).MakeGenericType(signatureTypes);
            case 4:
                return typeof(Func<,,,>).MakeGenericType(signatureTypes);
            case 5:
                return typeof(Func<,,,,>).MakeGenericType(signatureTypes);
            case 6:
                return typeof(Func<,,,,,>).MakeGenericType(signatureTypes);
            case 7:
                return typeof(Func<,,,,,,>).MakeGenericType(signatureTypes);
            case 8:
                return typeof(Func<,,,,,,,>).MakeGenericType(signatureTypes);
            case 9:
                return typeof(Func<,,,,,,,,>).MakeGenericType(signatureTypes);
            case 10:
                return typeof(Func<,,,,,,,,,>).MakeGenericType(signatureTypes);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static Type GetClosedDelegateType(MethodInfo info) {
        ParameterInfo[] parameters = info.GetParameters();
        Type[] signatureTypes = new Type[parameters.Length + 1];

        for (int i = 0; i < parameters.Length; i++) {
            signatureTypes[i] = parameters[i].ParameterType;
        }

        signatureTypes[parameters.Length + 1] = info.ReturnType;

        switch (signatureTypes.Length) {
            case 1:
                return typeof(Func<>).MakeGenericType(signatureTypes);
            case 2:
                return typeof(Func<,>).MakeGenericType(signatureTypes);
            case 3:
                return typeof(Func<,,>).MakeGenericType(signatureTypes);
            case 4:
                return typeof(Func<,,,>).MakeGenericType(signatureTypes);
            case 5:
                return typeof(Func<,,,,>).MakeGenericType(signatureTypes);
            case 6:
                return typeof(Func<,,,,,>).MakeGenericType(signatureTypes);
            case 7:
                return typeof(Func<,,,,,,>).MakeGenericType(signatureTypes);
            case 8:
                return typeof(Func<,,,,,,,>).MakeGenericType(signatureTypes);
            case 9:
                return typeof(Func<,,,,,,,,>).MakeGenericType(signatureTypes);
            case 10:
                return typeof(Func<,,,,,,,,,>).MakeGenericType(signatureTypes);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static bool SignatureMatches(MethodInfo one, MethodInfo two) {
        if (one.ReturnType != two.ReturnType) return false;
        if (one.Name != two.Name) return false;

        ParameterInfo[] parameters1 = one.GetParameters();
        ParameterInfo[] parameters2 = two.GetParameters();
        if (parameters1.Length != parameters2.Length) {
            return false;
        }

        for (int i = 0; i < parameters1.Length; i++) {
            if (parameters1[i].ParameterType != parameters2[i].ParameterType) {
                return false;
            }
        }

        return true;
    }

    private static MethodInfo ResolvePossibleInterface(Type declaringType, MethodInfo original) {
        if (original.IsStatic) {
            return original;
        }

        Type[] interfaces = declaringType.GetInterfaces();

        if (interfaces.Length == 0) {
            return original;
        }

        for (int i = 0; i < interfaces.Length; i++) {
            MethodInfo interfaceMethod = interfaces[i]
                .GetMethods(PublicInstance)
                .FirstOrDefault((methodInfo) => SignatureMatches(original, methodInfo));

            if (interfaceMethod != null) {
                return interfaceMethod;
            }
        }

        return original;
    }

    private static MethodInfo ResolvePossibleBaseClassMethod(Type declaringType, MethodInfo original) {
        Type baseClass = declaringType.BaseType;

        while (baseClass != null) {
            MethodInfo method = baseClass
                .GetMethods(PublicInstance)
                .FirstOrDefault((methodInfo) => SignatureMatches(original, methodInfo));

            if (method != null) {
                return method;
            }

            baseClass = baseClass.BaseType;
        }

        return original;
    }

    private static Delegate GetClosedDelegate(MethodInfo methodInfo) {
        for (int i = 0; i < staticDelegates.Count; i++) {
            DelegateEntry entry = staticDelegates[i];
            if (entry.methodInfo == methodInfo) {
                return entry.instance;
            }
        }

        Type delegateType = GetClosedDelegateType(methodInfo);
        Delegate instance = Delegate.CreateDelegate(delegateType, methodInfo, true);
        DelegateEntry newEntry = new DelegateEntry(instance, methodInfo);
        staticDelegates.Add(newEntry);
        return instance;
    }

    private static Delegate GetClosedDelegate(Type delegateType, MethodInfo methodInfo) {
        for (int i = 0; i < staticDelegates.Count; i++) {
            DelegateEntry entry = staticDelegates[i];
            if (entry.methodInfo == methodInfo) {
                return entry.instance;
            }
        }

        Delegate instance = Delegate.CreateDelegate(delegateType, methodInfo, true);
        DelegateEntry newEntry = new DelegateEntry(instance, methodInfo);
        staticDelegates.Add(newEntry);
        return instance;
    }

    private static Delegate GetOpenDelegate(MethodInfo methodInfo) {
        for (int i = 0; i < openDelegates.Count; i++) {
            DelegateEntry entry = openDelegates[i];
            if (entry.methodInfo == methodInfo) {
                return entry.instance;
            }
        }

        Type delegateType = GetOpenDelegateType(methodInfo);
        Delegate openDelegate = CreateOpenDelegate(delegateType, methodInfo);
        DelegateEntry openEntry = new DelegateEntry(openDelegate, methodInfo);
        openDelegates.Add(openEntry);

        return openDelegate;
    }

    private static Delegate GetOpenDelegate(Type delegateType, MethodInfo methodInfo) {
        for (int i = 0; i < openDelegates.Count; i++) {
            DelegateEntry entry = openDelegates[i];
            if (entry.methodInfo == methodInfo) {
                return entry.instance;
            }
        }

        Delegate openDelegate = CreateOpenDelegate(delegateType, methodInfo);
        DelegateEntry openEntry = new DelegateEntry(openDelegate, methodInfo);
        openDelegates.Add(openEntry);

        return openDelegate;
    }

    public static Delegate GetDelegate(MethodInfo methodInfo) {
        methodInfo = ResolvePossibleBaseClassMethod(methodInfo.DeclaringType, methodInfo);
        methodInfo = ResolvePossibleInterface(methodInfo.DeclaringType, methodInfo);
        return methodInfo.IsStatic ? GetClosedDelegate(methodInfo) : GetOpenDelegate(methodInfo);
    }

    public static Delegate GetDelegate(Type delegateType, MethodInfo methodInfo) {
        methodInfo = ResolvePossibleInterface(methodInfo.DeclaringType, methodInfo);
        return methodInfo.IsStatic ? GetClosedDelegate(delegateType, methodInfo) : GetOpenDelegate(delegateType, methodInfo);
    }

    public static Type GetFieldType(Type type, string fieldName) {
        return GetFieldInfoOrThrow(type, fieldName).FieldType;
    }

    private static Delegate CreateFieldGetter(Type declaredType, string fieldName) {
        ParameterExpression paramExpression = Expression.Parameter(declaredType, "value");
        Expression propertyGetterExpression = Expression.Property(paramExpression, fieldName);
        return Expression.Lambda(propertyGetterExpression, paramExpression).Compile();
    }

    private static Delegate CreateFieldSetter(Type baseType, Type fieldType, string fieldName) {
        ParameterExpression paramExpression0 = Expression.Parameter(baseType);
        ParameterExpression paramExpression1 = Expression.Parameter(fieldType, fieldName);
        MemberExpression fieldGetter = Expression.Field(paramExpression0, fieldName);

        return Expression.Lambda(
            Expression.Assign(fieldGetter, paramExpression1),
            paramExpression0,
            paramExpression1
        ).Compile();

    }

    public struct LinqAccessor {

        public readonly string fieldName;
        public readonly Delegate fieldSetter;
        public readonly Delegate fieldGetter;

        public LinqAccessor(string fieldName, Delegate fieldGetter, Delegate fieldSetter) {
            this.fieldName = fieldName;
            this.fieldGetter = fieldGetter;
            this.fieldSetter = fieldSetter;
        }

    }
    
    public static LinqAccessor GetLinqAccessors(Type baseType, Type fieldType, string fieldName) {
        List<LinqAccessor> linqList;

        if (linqDelegates.TryGetValue(baseType, out linqList)) {
            for (int i = 0; i < linqList.Count; i++) {
                if (linqList[i].fieldName == fieldName) {
                    return linqList[i];
                }
            }
        }
        else {
            linqList = new List<LinqAccessor>();
            linqDelegates[baseType] = linqList;
        }

        Delegate getter = CreateFieldGetter(baseType, fieldName);
        Delegate setter = CreateFieldSetter(baseType, fieldType, fieldName);
        LinqAccessor linqEntry = new LinqAccessor(fieldName, getter, setter);
        linqList.Add(linqEntry);
        
        return linqEntry;
    }

    public static Action<TObject, TProperty> GetPropSetter<TObject, TProperty>(string propertyName) {

        ParameterExpression paramExpression0 = Expression.Parameter(typeof(TObject));
        ParameterExpression paramExpression1 = Expression.Parameter(typeof(TProperty), propertyName);
        MemberExpression propertyGetterExpression = Expression.Field(paramExpression0, propertyName);

        return Expression.Lambda<Action<TObject, TProperty>>(
            Expression.Assign(propertyGetterExpression, paramExpression1),
            paramExpression0, 
            paramExpression1
        ).Compile();

    }
    
    public static Func<TObject, TProperty> GetPropGetter<TObject, TProperty>(string propertyName) {
        ParameterExpression paramExpression = Expression.Parameter(typeof(TObject), "value");
        Expression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
        return Expression.Lambda<Func<TObject, TProperty>>(propertyGetterExpression, paramExpression).Compile();
    }

}