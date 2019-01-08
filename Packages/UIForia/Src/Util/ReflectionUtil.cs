﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UIForia;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine;
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
    public const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
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
    public static readonly object[] ObjectArray4 = new object[4];
    public static readonly object[] ObjectArray5 = new object[5];

    public static readonly Type[] TypeArray1 = new Type[1];
    public static readonly Type[] TypeArray2 = new Type[2];
    public static readonly Type[] TypeArray3 = new Type[3];
    public static readonly Type[] TypeArray4 = new Type[4];

//    private static Dictionary<Type, object[]> methodAttributeCache;

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

    public static FieldInfo GetFieldInfo(Type type, string fieldName) {
        return type.GetField(fieldName, InstanceBindFlags);
    }

    public static FieldInfo GetStaticFieldInfo(Type type, string fieldName) {
        return type.GetField(fieldName, StaticFlags);
    }

    public static FieldInfo GetInstanceOrStaticFieldInfo(Type type, string fieldName) {
        return type.GetField(fieldName, StaticFlags | InstanceBindFlags);
    }

    public static PropertyInfo GetPropertyInfo(Type type, string propertyType) {
        return type.GetProperty(propertyType, InstanceBindFlags);
    }

    public static PropertyInfo GetStaticPropertyInfo(Type type, string fieldName) {
        return type.GetProperty(fieldName, StaticFlags);
    }

    public static PropertyInfo GetInstanceOrStaticPropertyInfo(Type type, string propertyType) {
        return type.GetProperty(propertyType, StaticFlags | InstanceBindFlags);
    }

    public static FieldInfo GetFieldInfoOrThrow(Type type, string fieldName) {
        FieldInfo fieldInfo = type.GetField(fieldName, InstanceBindFlags);
        if (fieldInfo == null) {
            throw new Exception($"Field called {fieldName} was not found on type {type.Name}");
        }

        return fieldInfo;
    }

    public static PropertyInfo GetPropertyInfoOrThrow(Type type, string propertyName) {
        PropertyInfo propertyInfo = type.GetProperty(propertyName, InstanceBindFlags);
        if (propertyInfo == null) {
            throw new Exception($"Property called {propertyName} was not found on type {type.Name}");
        }

        return propertyInfo;
    }

    public static bool IsField(Type type, string fieldName) {
        return type.GetField(fieldName, InstanceBindFlags) != null;
    }

    public static bool IsProperty(Type type, string propertyName) {
        return type.GetProperty(propertyName, InstanceBindFlags) != null;
    }

    public static Type GetFieldType(Type type, string fieldName) {
        return GetFieldInfoOrThrow(type, fieldName).FieldType;
    }

    public static Type GetPropertyType(Type type, string propertyName) {
        return GetPropertyInfoOrThrow(type, propertyName).PropertyType;
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
                    case TypeCode.Byte: return true;
                    case TypeCode.Int16: return true;
                    case TypeCode.Int32: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Double: return true;
                    case TypeCode.Single: return true;
                    default: return false;
                }
            case TypeCode.Int16:
                switch (Type.GetTypeCode(right)) {
                    case TypeCode.Byte: return false;
                    case TypeCode.Int16: return true;
                    case TypeCode.Int32: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Double: return true;
                    case TypeCode.Single: return true;
                    default: return false;
                }
            case TypeCode.Int32:
                switch (Type.GetTypeCode(right)) {
                    case TypeCode.Byte: return false;
                    case TypeCode.Int16: return false;
                    case TypeCode.Int32: return true;
                    case TypeCode.Int64: return true;
                    case TypeCode.Double: return true;
                    case TypeCode.Single: return true;
                    default: return false;
                }
            case TypeCode.Int64:
                switch (Type.GetTypeCode(right)) {
                    case TypeCode.Byte: return false;
                    case TypeCode.Int16: return false;
                    case TypeCode.Int32: return false;
                    case TypeCode.Int64: return true;
                    case TypeCode.Double: return true;
                    case TypeCode.Single: return true;
                    default: return false;
                }
            case TypeCode.Double:
                switch (Type.GetTypeCode(right)) {
                    case TypeCode.Byte: return false;
                    case TypeCode.Int16: return false;
                    case TypeCode.Int32: return true;
                    case TypeCode.Int64: return false;
                    case TypeCode.Double: return true;
                    case TypeCode.Single: return true;
                    default: return false;
                }
            case TypeCode.Single:
                switch (Type.GetTypeCode(right)) {
                    case TypeCode.Byte: return false;
                    case TypeCode.Int16: return false;
                    case TypeCode.Int32: return true;
                    case TypeCode.Int64: return false;
                    case TypeCode.Double: return false;
                    case TypeCode.Single: return true;
                    default: return false;
                }
            default:
                return false;
        }
    }

    public static Type ResolveFieldOrPropertyType(Type type, string name) {
        FieldInfo fieldInfo = GetInstanceOrStaticFieldInfo(type, name);
        if (fieldInfo != null) {
            return fieldInfo.FieldType;
        }

        PropertyInfo propertyInfo = GetInstanceOrStaticPropertyInfo(type, name);

        if (propertyInfo != null) {
            return propertyInfo.PropertyType;
        }

        return null;
    }

    public static bool IsOverride(MethodInfo m) {
        return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
    }

    // todo -- doesn't warn about parameters, etc
    public static bool IsOverride(object target, string methodName) {
        MethodInfo info = target.GetType().GetMethod(methodName, PublicInstance, null, Type.EmptyTypes, null);
        return IsOverride(info);
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

        int additionalSize = info.ReturnType == typeof(void) ? 1 : 2;

        Type[] signatureTypes = new Type[parameters.Length + additionalSize];

        signatureTypes[0] = info.DeclaringType;

        for (int i = 1; i < parameters.Length + 1; i++) {
            signatureTypes[i] = parameters[i - 1].ParameterType;
        }

        if (info.ReturnType != typeof(void)) {
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

        switch (signatureTypes.Length) {
            case 1:
                return typeof(Action<>).MakeGenericType(signatureTypes);
            case 2:
                return typeof(Action<,>).MakeGenericType(signatureTypes);
            case 3:
                return typeof(Action<,,>).MakeGenericType(signatureTypes);
            case 4:
                return typeof(Action<,,,>).MakeGenericType(signatureTypes);
            case 5:
                return typeof(Action<,,,,>).MakeGenericType(signatureTypes);
            case 6:
                return typeof(Action<,,,,,>).MakeGenericType(signatureTypes);
            case 7:
                return typeof(Action<,,,,,,>).MakeGenericType(signatureTypes);
            case 8:
                return typeof(Action<,,,,,,,>).MakeGenericType(signatureTypes);
            case 9:
                return typeof(Action<,,,,,,,,>).MakeGenericType(signatureTypes);
            case 10:
                return typeof(Action<,,,,,,,,,>).MakeGenericType(signatureTypes);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static Type GetClosedDelegateType(MethodInfo info) {
        ParameterInfo[] parameters = info.GetParameters();

        int additionalSize = info.ReturnType == typeof(void) ? 0 : 1;

        Type[] signatureTypes = new Type[parameters.Length + additionalSize];

        for (int i = 0; i < parameters.Length; i++) {
            signatureTypes[i] = parameters[i].ParameterType;
        }

        if (info.ReturnType != typeof(void)) {
            signatureTypes[parameters.Length] = info.ReturnType;

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

        switch (signatureTypes.Length) {
            case 0:
                return typeof(Action);
            case 1:
                return typeof(Action<>).MakeGenericType(signatureTypes);
            case 2:
                return typeof(Action<,>).MakeGenericType(signatureTypes);
            case 3:
                return typeof(Action<,,>).MakeGenericType(signatureTypes);
            case 4:
                return typeof(Action<,,,>).MakeGenericType(signatureTypes);
            case 5:
                return typeof(Action<,,,,>).MakeGenericType(signatureTypes);
            case 6:
                return typeof(Action<,,,,,>).MakeGenericType(signatureTypes);
            case 7:
                return typeof(Action<,,,,,,>).MakeGenericType(signatureTypes);
            case 8:
                return typeof(Action<,,,,,,,>).MakeGenericType(signatureTypes);
            case 9:
                return typeof(Action<,,,,,,,,>).MakeGenericType(signatureTypes);
            case 10:
                return typeof(Action<,,,,,,,,,>).MakeGenericType(signatureTypes);
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

    private static Delegate CreateFieldGetter(Type declaredType, string fieldName) {
        ParameterExpression paramExpression = Expression.Parameter(declaredType, "value");
        Expression fieldGetterExpression = Expression.Field(paramExpression, fieldName);
        return Expression.Lambda(fieldGetterExpression, paramExpression).Compile();
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
        public readonly Delegate setter;
        public readonly Delegate getter;

        public LinqAccessor(string fieldName, Delegate getter, Delegate setter) {
            this.fieldName = fieldName;
            this.getter = getter;
            this.setter = setter;
        }

    }

    public static LinqAccessor GetLinqPropertyAccessors(Type baseType, PropertyInfo propertyInfo) {
        return GetLinqPropertyAccessors(baseType, propertyInfo.PropertyType, propertyInfo.Name);
    }

    public static LinqAccessor GetLinqPropertyAccessors(Type baseType, Type propertyType, string propertyName) {
        List<LinqAccessor> linqList;

        if (linqDelegates.TryGetValue(baseType, out linqList)) {
            for (int i = 0; i < linqList.Count; i++) {
                if (linqList[i].fieldName == propertyName) {
                    return linqList[i];
                }
            }
        }
        else {
            linqList = new List<LinqAccessor>();
            linqDelegates[baseType] = linqList;
        }

        Delegate getter = CreatePropertyGetter(baseType, propertyName);
        Delegate setter = CreatePropertySetter(baseType, propertyType, propertyName);
        LinqAccessor linqEntry = new LinqAccessor(propertyName, getter, setter);
        linqList.Add(linqEntry);

        return linqEntry;
    }

    public static LinqAccessor GetLinqFieldAccessors(Type baseType, FieldInfo fieldInfo) {
        return GetLinqFieldAccessors(baseType, fieldInfo.FieldType, fieldInfo.Name);
    }

    public static LinqAccessor GetLinqFieldAccessors(Type baseType, Type fieldType, string fieldName) {
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

    public static Delegate CreatePropertySetter(Type objectType, Type propertyType, string propertyName) {
        try {
            ParameterExpression paramExpression0 = Expression.Parameter(objectType);
            ParameterExpression paramExpression1 = Expression.Parameter(propertyType, propertyName);
            MemberExpression propertyGetterExpression = Expression.Property(paramExpression0, propertyName);

            return Expression.Lambda(
                Expression.Assign(propertyGetterExpression, paramExpression1),
                paramExpression0,
                paramExpression1
            ).Compile();
        }
        catch (Exception ex) {
            return null;
        }
    }

    public static Delegate CreateStaticPropertyGetter(Type objectType, string propertyName) {
        Expression propertyGetterExpression = Expression.Property(null, objectType, propertyName);
        return Expression.Lambda(propertyGetterExpression, null).Compile();
    }

    public static Delegate CreateStaticFieldGetter(Type objectType, string propertyName) {
        Expression propertyGetterExpression = Expression.Field(null, objectType, propertyName);
        return Expression.Lambda(propertyGetterExpression, null).Compile();
    }

    public static Delegate CreatePropertyGetter(Type objectType, string propertyName) {
        ParameterExpression paramExpression = Expression.Parameter(objectType, "value");
        Expression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
        return Expression.Lambda(propertyGetterExpression, paramExpression).Compile();
    }

    // todo -- need some parameter matching at least
    public static MethodInfo GetMethodInfo(Type type, string methodName) {
        return type.GetMethod(methodName, StaticFlags | InstanceBindFlags);
    }

    public static bool IsCallbackType(Type type) {
        if (type == typeof(Action)) return true;
        Type generic = null;
        if (type.IsGenericTypeDefinition) {
            generic = type;
        }
        else if (type.IsGenericType) {
            generic = type.GetGenericTypeDefinition();
        }

        if (generic == null) return false;
        if (generic == typeof(Action<>)) return true;
        if (generic == typeof(Action<,>)) return true;
        if (generic == typeof(Action<,,>)) return true;
        if (generic == typeof(Action<,,,>)) return true;
        if (generic == typeof(Action<,,,,>)) return true;
        if (generic == typeof(Action<,,,,,>)) return true;
        if (generic == typeof(Action<,,,,,,>)) return true;
        if (generic == typeof(Func<>)) return true;
        if (generic == typeof(Func<,>)) return true;
        if (generic == typeof(Func<,,>)) return true;
        if (generic == typeof(Func<,,,>)) return true;
        if (generic == typeof(Func<,,,,>)) return true;
        if (generic == typeof(Func<,,,,,>)) return true;
        if (generic == typeof(Func<,,,,,,>)) return true;
        if (generic == typeof(Func<,,,,,,,>)) return true;
        return false;
    }

    public static bool IsPropertyStatic(PropertyInfo propertyInfo) {
        return propertyInfo.GetMethod?.IsStatic ?? false;
    }

    public static bool IsPropertyReadOnly(PropertyInfo propertyInfo) {
        return propertyInfo.SetMethod == null;
    }

    public static bool IsAction(Type type) {
        if (type == typeof(Action)) {
            return true;
        }

        Type generic = null;
        if (type.IsGenericTypeDefinition) {
            generic = type;
        }
        else if (type.IsGenericType) {
            generic = type.GetGenericTypeDefinition();
        }

        if (generic == typeof(Action<>)) return true;
        if (generic == typeof(Action<,>)) return true;
        if (generic == typeof(Action<,,>)) return true;
        if (generic == typeof(Action<,,,>)) return true;
        if (generic == typeof(Action<,,,,>)) return true;
        if (generic == typeof(Action<,,,,,>)) return true;
        if (generic == typeof(Action<,,,,,,>)) return true;
        return false;
    }

    public static MethodInfo GetImplicitConversion(Type targetType, Type inputType) {
        MethodInfo[] infos = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < infos.Length; i++) {
            if (infos[i].Name == "op_Implicit" && infos[i].ReturnType == targetType) {
                ParameterInfo pi = infos[i].GetParameters().FirstOrDefault();
                if (pi != null && pi.ParameterType == inputType) {
                    return infos[i];
                }
            }
        }

        return null;
    }

    public static MethodInfo GetBinaryOperator(string opName, Type leftType, Type rightType) {
        MethodInfo[] infos = leftType.GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < infos.Length; i++) {
            if (infos[i].Name == opName && infos[i].ReturnType == leftType) {
                ParameterInfo[] pi = infos[i].GetParameters();
                if (pi.Length != 2) {
                    continue;
                }

                if (pi[0].ParameterType == leftType && pi[1].ParameterType == rightType) {
                    return infos[i];
                }
            }
        }

        return null;
    }

    // todo don't require bool return type
    public static MethodInfo GetComparisonOperator(string opName, Type leftType, Type rightType) {
        MethodInfo[] infos = leftType.GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < infos.Length; i++) {
            if (infos[i].Name == opName && infos[i].ReturnType == typeof(bool)) {
                ParameterInfo[] pi = infos[i].GetParameters();
                if (pi.Length != 2) {
                    continue;
                }

                if (pi[0].ParameterType == leftType && pi[1].ParameterType == rightType) {
                    return infos[i];
                }
            }
        }

        return null;
    }

    public static MethodInfo GetUnaryOperator(string opName, Type type) {
        MethodInfo[] infos = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < infos.Length; i++) {
            if (infos[i].Name == opName) {
                ParameterInfo[] pi = infos[i].GetParameters();
                if (pi.Length != 1) {
                    continue;
                }

                if (pi[0].ParameterType == type) {
                    return infos[i];
                }
            }
        }

        return null;
    }

    public static List<MethodInfo> GetMethodsWithName(Type type, string targetName) {
        MethodInfo[] infos = type.GetMethods(InstanceBindFlags | StaticFlags);
        List<MethodInfo> retn = new List<MethodInfo>();
        for (int i = 0; i < infos.Length; i++) {
            if (infos[i].Name == targetName) {
                retn.Add(infos[i]);
            }
        }

        return retn;
    }

    public static bool IsConstantField(Type rootType, string fieldName) {
        FieldInfo info = rootType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        return info != null && info.IsLiteral && !info.IsInitOnly;
    }

    public static FieldInfo GetConstantField(Type rootType, string fieldName) {
        return rootType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
    }
}

public struct ConstructorArguments {

    public readonly object arg0;
    public readonly object arg1;
    public readonly object arg2;
    public readonly object arg3;
    public readonly object arg4;
    public int count;

    public ConstructorArguments(object arg0) {
        this.arg0 = arg0;
        this.arg1 = null;
        this.arg2 = null;
        this.arg3 = null;
        this.arg4 = null;
        count = 1;
    }

    public ConstructorArguments(object arg0, object arg1) {
        this.arg0 = arg0;
        this.arg1 = arg1;
        this.arg2 = null;
        this.arg3 = null;
        this.arg4 = null;
        count = 2;
    }

    public ConstructorArguments(object arg0, object arg1, object arg2) {
        this.arg0 = arg0;
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.arg3 = null;
        this.arg4 = null;
        count = 3;
    }

    public ConstructorArguments(object arg0, object arg1, object arg2, object arg3) {
        this.arg0 = arg0;
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.arg3 = arg3;
        this.arg4 = null;
        count = 4;
    }

    public ConstructorArguments(object arg0, object arg1, object arg2, object arg3, object arg4) {
        this.arg0 = arg0;
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.arg3 = arg3;
        this.arg4 = arg4;
        count = 5;
    }

    public object[] GetArguments() {
        switch (count) {
            case 0:
                return ReflectionUtil.ObjectArray0;
            case 1:
                ReflectionUtil.ObjectArray1[0] = arg0;
                return ReflectionUtil.ObjectArray1;
            case 2:
                ReflectionUtil.ObjectArray2[0] = arg0;
                ReflectionUtil.ObjectArray2[1] = arg1;
                return ReflectionUtil.ObjectArray2;
            case 3:
                ReflectionUtil.ObjectArray3[0] = arg0;
                ReflectionUtil.ObjectArray3[1] = arg1;
                ReflectionUtil.ObjectArray3[2] = arg2;
                return ReflectionUtil.ObjectArray3;
            case 4:
                ReflectionUtil.ObjectArray4[0] = arg0;
                ReflectionUtil.ObjectArray4[1] = arg1;
                ReflectionUtil.ObjectArray4[2] = arg2;
                ReflectionUtil.ObjectArray4[3] = arg3;
                return ReflectionUtil.ObjectArray4;
            case 5:
                ReflectionUtil.ObjectArray5[0] = arg0;
                ReflectionUtil.ObjectArray5[1] = arg1;
                ReflectionUtil.ObjectArray5[2] = arg2;
                ReflectionUtil.ObjectArray5[3] = arg3;
                ReflectionUtil.ObjectArray5[4] = arg4;
                return ReflectionUtil.ObjectArray5;
        }

        return null;
    }


    public static implicit operator object[](ConstructorArguments arguments) {
        return arguments.GetArguments();
    }

}

public struct GenericArguments {

    public readonly Type arg0;
    public readonly Type arg1;
    public readonly Type arg2;
    public readonly Type arg3;
    public int count;

    public GenericArguments(Type arg0) {
        this.arg0 = arg0;
        this.arg1 = null;
        this.arg2 = null;
        this.arg3 = null;
        count = 1;
    }

    public GenericArguments(Type arg0, Type arg1) {
        this.arg0 = arg0;
        this.arg1 = arg1;
        this.arg2 = null;
        this.arg3 = null;
        count = 2;
    }

    public GenericArguments(Type arg0, Type arg1, Type arg2) {
        this.arg0 = arg0;
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.arg3 = null;
        count = 3;
    }

    public GenericArguments(Type arg0, Type arg1, Type arg2, Type arg3) {
        this.arg0 = arg0;
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.arg3 = arg3;
        count = 4;
    }

    public Type[] GetArguments() {
        switch (count) {
            case 1:
                ReflectionUtil.TypeArray1[0] = arg0;
                return ReflectionUtil.TypeArray1;
            case 2:
                ReflectionUtil.TypeArray2[0] = arg0;
                ReflectionUtil.TypeArray2[1] = arg1;
                return ReflectionUtil.TypeArray2;
            case 3:
                ReflectionUtil.TypeArray3[0] = arg0;
                ReflectionUtil.TypeArray3[1] = arg1;
                ReflectionUtil.TypeArray3[2] = arg2;
                return ReflectionUtil.TypeArray3;
            case 4:
                ReflectionUtil.TypeArray4[0] = arg0;
                ReflectionUtil.TypeArray4[1] = arg1;
                ReflectionUtil.TypeArray4[2] = arg2;
                ReflectionUtil.TypeArray4[3] = arg3;
                return ReflectionUtil.TypeArray4;
        }

        return null;
    }

    public static implicit operator Type[](GenericArguments arguments) {
        return arguments.GetArguments();
    }

}