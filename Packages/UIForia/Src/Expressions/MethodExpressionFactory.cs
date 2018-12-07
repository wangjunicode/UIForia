using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Compilers;
using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia {

    public static class MethodExpressionFactory {

        [Flags]
        public enum MethodType {

            Static = 1 << 0,
            Instance = 1 << 1,
            Void = 1 << 2,

            InstanceVoid = Instance | Void,
            StaticVoid = Static | Void

        }

        public static Expression CreateFuncExpression(List<Expression> arguments) {
            return null;
        }
        
        public static Expression CreateMethodExpression(MethodInfo info, List<Expression> arguments) {
            MethodType methodType = info.IsStatic ? MethodType.Static : MethodType.Instance;

            if (info.ReturnType == typeof(void)) {
                methodType |= MethodType.Void;
            }

            int genericOffset = 0;
            int extraArgumentCount = 2;

            if ((methodType & MethodType.Void) != 0) {
                extraArgumentCount--;
            }

            if ((methodType & MethodType.Static) != 0) {
                extraArgumentCount--;
            }

            // This wasn't working when array came from a pool.
            // Not sure why, could be that the delegate method it gets input to retains a reference
            Type[] genericArguments = new Type[arguments.Count + extraArgumentCount]; 

            if ((methodType & MethodType.Instance) != 0) {
                genericArguments[0] = info.DeclaringType;
                genericOffset = 1;
            }

            if ((methodType & MethodType.Void) == 0) {
                genericArguments[genericArguments.Length - 1] = info.ReturnType;
            }

            for (int i = 0; i < arguments.Count; i++) {
                genericArguments[i + genericOffset] = arguments[i].YieldedType;
            }

            Type callType = GetMethodCallType(info, arguments.Count, genericArguments, methodType);

            ReflectionUtil.ObjectArray2[0] = info;
            ReflectionUtil.ObjectArray2[1] = arguments.ToArray();

            return (Expression) ReflectionUtil.CreateGenericInstance(callType, ReflectionUtil.ObjectArray2);
        }
        
        public static Type GetMethodCallType(MethodInfo info, int argumentCount, Type[] genericArguments, MethodType methodType) {
            switch (argumentCount) {
                case 0:

                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<>), genericArguments);
                        case MethodType.StaticVoid:
                            return typeof(MethodCallExpression_StaticVoid);
                        case MethodType.InstanceVoid:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<>), genericArguments);
                        case MethodType.Instance:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,>), genericArguments);
                        default:
                            throw new CompileException("Cannot create method");
                    }
                case 1:
                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,>), genericArguments);
                        case MethodType.StaticVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<>), genericArguments);
                        case MethodType.InstanceVoid:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,>), genericArguments);
                        case MethodType.Instance:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,>), genericArguments);
                        default:
                            throw new CompileException("Cannot create method");
                    }
                case 2:
                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,>), genericArguments);
                        case MethodType.StaticVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,>), genericArguments);
                        case MethodType.InstanceVoid:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,>), genericArguments);
                        case MethodType.Instance:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,>), genericArguments);
                        default:
                            throw new CompileException("Cannot create method");
                    }
                case 3:
                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,>), genericArguments);
                        case MethodType.StaticVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,,>), genericArguments);
                        case MethodType.InstanceVoid:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,,>), genericArguments);
                        case MethodType.Instance:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,,>), genericArguments);
                        default:
                            throw new CompileException("Cannot create method");
                    }

                case 4:
                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,,>), genericArguments);
                        case MethodType.StaticVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,,,>), genericArguments);
                        case MethodType.InstanceVoid:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,,,>), genericArguments);
                        case MethodType.Instance:
                            if (genericArguments[0].IsValueType) {
                                throw new CompileException("Struct types cannot have methods called on them in templates. This is a limitation of C# reflection");
                            }
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,,,>), genericArguments);
                        default:
                            throw new CompileException("Cannot create method");
                    }

                default:
                    throw CompileExceptions.TooManyArgumentsException(info.Name, argumentCount);
            }
        }

    }

}