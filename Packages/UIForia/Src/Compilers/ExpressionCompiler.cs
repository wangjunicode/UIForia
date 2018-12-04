//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Reflection;
//using JetBrains.Annotations;
//using UIForia.Compilers;
//using UIForia.Compilers.CastHandlers;
//using UIForia.Parsing;
//using UIForia.Util;
//using UnityEngine;
//
//namespace UIForia {
//
//    // takes the place of void since generics can't use void types
//
//    public class ExpressionCompiler {
//
//        public ContextDefinition context;
//        private List<ICastHandler> userCastHandlers;
//        private LightList<ExpressionAliasResolver> expressionAliasResolvers;
//
//        private static readonly List<ICastHandler> builtInCastHandlers = new List<ICastHandler>() {
//            new CastHandler_ToString(),
//            new CastHandler_ColorToVector4(),
//            new CastHandler_DoubleToMeasurement(),
//            new CastHandler_FloatToInt(),
//            new CastHandler_FloatToMeasurement(),
//            new CastHandler_IntToDouble(),
//            new CastHandler_IntToFloat(),
//            new CastHandler_IntToMeasurement(),
//            new CastHandler_FloatToFixedLength(),
//            new CastHandler_DoubleToFixedLength(),
//            new CastHandler_IntToFixedLength(),
//            new CastHandler_Vector2ToVector3(),
//            new CastHandler_Vector3ToVector2(),
//            new CastHandler_Vector4ToColor()
//        };
//
//        [PublicAPI]
//        public ExpressionCompiler(ContextDefinition context) {
//            this.context = context;
//        }
//
//        [PublicAPI]
//        public Expression Compile(ExpressionNodeOld root) {
//            return Visit(root);
//        }
//
//        [PublicAPI]
//        public Expression<T> Compile<T>(ExpressionNodeOld root) {
//            return (Expression<T>) HandleCasting(typeof(T), Visit(root));
//        }
//
//        [PublicAPI]
//        public Expression Compile(string source) {
//            try {
//                return Visit(ExpressionParser.Parse(source));
//            }
//            catch (Exception e) {
//                Debug.Log("Error compiling: " + source);
//                Debug.Log(e.StackTrace);
//                throw e;
//            }
//        }
//
//        [PublicAPI]
//        public Expression Compile(Type outputType, string source) {
//            return HandleCasting(outputType, Visit(ExpressionParser.Parse(source)));
//        }
//
//        [PublicAPI]
//        public Expression<T> Compile<T>(string source) {
//            return (Expression<T>) HandleCasting(typeof(T), Visit(ExpressionParser.Parse(source)));
//        }
//
//        [PublicAPI]
//        public void AddExpressionResolver(ExpressionAliasResolver resolver) {
//            expressionAliasResolvers = expressionAliasResolvers ?? new LightList<ExpressionAliasResolver>(4);
//            for (int i = 0; i < expressionAliasResolvers.Count; i++) {
//                if (expressionAliasResolvers[i].aliasName == resolver.aliasName) {
//                    throw new ParseException("Duplicate alias registered: " + resolver.aliasName);
//                }
//            }
//
//            expressionAliasResolvers.Add(resolver);
//        }
//
//        [PublicAPI]
//        public void RemoveExpressionResolver(ExpressionAliasResolver resolver) {
//            expressionAliasResolvers?.Remove(resolver);
//        }
//
//        [PublicAPI]
//        public void AddCastHandler(ICastHandler handler) {
//            if (userCastHandlers == null) {
//                userCastHandlers = new List<ICastHandler>();
//            }
//
//            userCastHandlers.Add(handler);
//        }
//
//        [PublicAPI]
//        public void RemoveCastHandler(ICastHandler handler) {
//            userCastHandlers?.Remove(handler);
//        }
//
//        [PublicAPI]
//        public void SetContext(ContextDefinition contextDefinition) {
//            this.context = contextDefinition;
//        }
//
//        private Expression Visit(ExpressionNodeOld nodeOld) {
//            switch (nodeOld.expressionType) {
//                case ExpressionNodeType.AliasAccessor:
//                    return VisitAliasNode((AliasExpressionNodeOld) nodeOld);
//
//                case ExpressionNodeType.Paren:
//                    return VisitParenNode((ParenExpressionNodeOld) nodeOld);
//
//                case ExpressionNodeType.RootContextAccessor:
//                    return VisitRootContextAccessor((RootContextLookupNodeOld) nodeOld);
//
//                case ExpressionNodeType.LiteralValue:
//                    return VisitConstant((LiteralValueNodeOld) nodeOld);
//
//                case ExpressionNodeType.Accessor:
//                    return VisitAccessExpression((AccessExpressionNodeOld) nodeOld);
//
//                case ExpressionNodeType.Unary:
//                    return VisitUnaryExpression((UnaryExpressionNodeOld) nodeOld);
//
//                case ExpressionNodeType.Operator:
//                    return VisitOperatorExpression((OperatorExpressionNodeOld) nodeOld);
//
//                case ExpressionNodeType.MethodCall:
//                    return VisitMethodCallExpression((MethodCallNodeOld) nodeOld);
//            }
//
//            return null;
//        }
//
//        private Expression HandleCasting(Type requiredType, Expression expression) {
//            Type yieldedType = expression.YieldedType;
//
//            if (yieldedType == requiredType) {
//                return expression;
//            }
//
//            if (userCastHandlers != null) {
//                for (int i = 0; i < userCastHandlers.Count; i++) {
//                    if (userCastHandlers[i].CanHandle(requiredType, yieldedType)) {
//                        return userCastHandlers[i].Cast(requiredType, expression);
//                    }
//                }
//            }
//
//            for (int i = 0; i < builtInCastHandlers.Count; i++) {
//                if (builtInCastHandlers[i].CanHandle(requiredType, yieldedType)) {
//                    return builtInCastHandlers[i].Cast(requiredType, expression);
//                }
//            }
//
//            return expression;
//        }
//
//        private static void ValidateParameterTypes(Type[] parameters, Expression[] arguments) {
//            for (int i = 0; i < parameters.Length; i++) {
//                if (!parameters[i].IsAssignableFrom(arguments[i].YieldedType)) {
//                    throw new Exception($"Cannot use parameter of type {arguments[i].YieldedType} for parameter of type {parameters[i]}");
//                }
//            }
//        }
//
//        private Expression VisitMethodAliasExpression(string alias, MethodCallNodeOld nodeOld) {
//            if (expressionAliasResolvers != null) {
//                for (int i = 0; i < expressionAliasResolvers.Count; i++) {
//                    if (expressionAliasResolvers[i].aliasName == alias) {
//                        Expression retn = expressionAliasResolvers[i].CompileAsMethodExpression(nodeOld, Visit);
//                        if (retn == null) {
//                            throw new ParseException($"Resolver {expressionAliasResolvers[i]} failed to parse {alias}");
//                        }
//
//                        return retn;
//                    }
//                }
//            }
//
//            throw new ParseException();
//        }
//
//        private struct Parameter {
//
//            public Type type;
//            public Expression expression;
//
//        }
//
//        private struct MethodCall {
//
//            public Type returnType;
//            public int parameterCount;
//            public Parameter p0;
//            public bool isStatic;
//
//        }
//
//        private Expression VisitMethodCallExpression(MethodCallNodeOld nodeOld) {
//            string methodName = nodeOld.identifierNodeOld.identifier;
//
//            if (methodName[0] == '$') {
//                return VisitMethodAliasExpression(methodName, nodeOld);
//            }
//
//            MethodInfo info = ReflectionUtil.GetMethodInfo(context.rootType, methodName);
//
//            if (info == null) {
//                throw new Exception($"Cannot find method {methodName} on type {context.rootType.Name} or any registered aliases");
//            }
//
//            MethodType methodType = 0;
//            bool isVoid = info.ReturnType == typeof(void);
//
//            if (info.IsStatic) {
//                methodType |= MethodType.Static;
//            }
//            else {
//                methodType |= MethodType.Instance;
//            }
//
//            if (isVoid) {
//                methodType |= MethodType.Void;
//            }
//
//            ParameterInfo[] parameters = info.GetParameters();
//            IReadOnlyList<ExpressionNodeOld> signatureParts = nodeOld.signatureNodeOld.parts;
//
//            if (parameters.Length != signatureParts.Count) {
//                throw new Exception("Argument count is wrong for method "
//                                    + methodName + " expected: " + parameters.Length
//                                    + " but was provided: " + signatureParts.Count);
//            }
//
//            int genericOffset = 0;
//            int extraArgumentCount = 2;
//
//            if ((methodType & MethodType.Void) != 0) {
//                extraArgumentCount--;
//            }
//
//            if ((methodType & MethodType.Static) != 0) {
//                extraArgumentCount--;
//            }
//
//            Expression[] args = new Expression[signatureParts.Count];
//            Type[] genericArguments = new Type[signatureParts.Count + extraArgumentCount];
//            Type[] parameterTypes = new Type[parameters.Length];
//
//            if ((methodType & MethodType.Instance) != 0) {
//                genericArguments[0] = context.rootType; // todo -- this means root functions only, no chaining right now! 
//                genericOffset = 1;
//            }
//
//            for (int i = 0; i < args.Length; i++) {
//                Type requiredType = parameters[i].ParameterType;
//                parameterTypes[i] = requiredType;
//                ExpressionNodeOld argumentNodeOld = signatureParts[i];
//                Expression argumentExpression = Visit(argumentNodeOld);
//                args[i] = HandleCasting(requiredType, argumentExpression);
//
//                genericArguments[i + genericOffset] = args[i].YieldedType;
//            }
//
//            if ((methodType & MethodType.Void) == 0) {
//                genericArguments[genericArguments.Length - 1] = info.ReturnType;
//            }
//
//            ValidateParameterTypes(parameterTypes, args);
//
//            Type callType = GetMethodCallType(methodName, args.Length, genericArguments, methodType);
//
//            ReflectionUtil.ObjectArray2[0] = info;
//            ReflectionUtil.ObjectArray2[1] = args;
//
//            return (Expression) ReflectionUtil.CreateGenericInstance(callType, ReflectionUtil.ObjectArray2);
//        }
//
//        private static Type GetMethodCallType(string methodName, int argumentCount, Type[] genericArguments, MethodType methodType) {
//            switch (argumentCount) {
//                case 0:
//
//                    switch (methodType) {
//                        case MethodType.Static:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<>), genericArguments);
//                        case MethodType.StaticVoid:
//                            return typeof(MethodCallExpression_StaticVoid);
//                        case MethodType.InstanceVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<>), genericArguments);
//                        case MethodType.Instance:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,>), genericArguments);
//                        default:
//                            throw new Exception("Cannot create method");
//                    }
//                case 1:
//                    switch (methodType) {
//                        case MethodType.Static:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,>), genericArguments);
//                        case MethodType.StaticVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<>), genericArguments);
//                        case MethodType.InstanceVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,>), genericArguments);
//                        case MethodType.Instance:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,>), genericArguments);
//                        default:
//                            throw new Exception("Cannot create method");
//                    }
//                case 2:
//                    switch (methodType) {
//                        case MethodType.Static:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,>), genericArguments);
//                        case MethodType.StaticVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,>), genericArguments);
//                        case MethodType.InstanceVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,>), genericArguments);
//                        case MethodType.Instance:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,>), genericArguments);
//                        default:
//                            throw new Exception("Cannot create method");
//                    }
//                case 3:
//                    switch (methodType) {
//                        case MethodType.Static:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,>), genericArguments);
//                        case MethodType.StaticVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,,>), genericArguments);
//                        case MethodType.InstanceVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,,>), genericArguments);
//                        case MethodType.Instance:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,,>), genericArguments);
//                        default:
//                            throw new Exception("Cannot create method");
//                    }
//
//                case 4:
//                    switch (methodType) {
//                        case MethodType.Static:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,,>), genericArguments);
//                        case MethodType.StaticVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,,,>), genericArguments);
//                        case MethodType.InstanceVoid:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,,,>), genericArguments);
//                        case MethodType.Instance:
//                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,,,>), genericArguments);
//                        default:
//                            throw new Exception("Cannot create method");
//                    }
//
//                default:
//                    throw new Exception(
//                        $"Expressions only support functions with up to 4 arguments. {methodName} is supplying {argumentCount} ");
//            }
//        }
//
//        private Expression VisitAliasNode(AliasExpressionNodeOld nodeOld) {
////            Type aliasedType = context.ResolveRuntimeAliasType(node.alias);
////
////            if (aliasedType == null) {
////                throw new Exception("Unable to resolve alias: " + node.alias);
////            }
//
//            string contextName = nodeOld.alias;
//            if (contextName.StartsWith("$") && expressionAliasResolvers != null) {
//                for (int i = 0; i < expressionAliasResolvers.Count; i++) {
//                    if (contextName == expressionAliasResolvers[i].aliasName) {
//                        Expression retn = expressionAliasResolvers[i].CompileAsValueExpression(context, nodeOld, Visit);
//                        if (retn == null) {
//                            throw new ParseException($"Resolver {expressionAliasResolvers[i]} failed to parse {contextName}");
//                        }
//
//                        return retn;
//                    }
//                }
//            }
//
//            throw new ParseException($"Unable to resolve alias {contextName}");
////            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
////                typeof(ResolveExpression_Alias<>),
////                aliasedType,
////                node.alias
////            );
//        }
//
//        private Expression VisitRootContextAccessor(RootContextLookupNodeOld nodeOld) {
//            string fieldName = nodeOld.idNodeOld.identifier;
////            // todo -- alias is resolved before field access, might be an issue
////            object constantAlias = context.ResolveConstAlias(fieldName);
////            if (constantAlias != null) {
////                Type aliasType = constantAlias.GetType();
////
////                ReflectionUtil.ObjectArray1[0] = constantAlias;
////                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
////                    typeof(ConstantExpression<>),
////                    aliasType,
////                    ReflectionUtil.ObjectArray1
////                );
////            }
//
//            if (ReflectionUtil.IsField(context.rootType, fieldName)) {
//                Type fieldType = ReflectionUtil.GetFieldType(context.rootType, fieldName);
//                ReflectionUtil.ObjectArray2[0] = context.rootType;
//                ReflectionUtil.ObjectArray2[1] = fieldName;
//                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                    typeof(AccessExpression_RootField<>),
//                    fieldType,
//                    ReflectionUtil.ObjectArray2
//                );
//            }
//
//            if (ReflectionUtil.IsProperty(context.rootType, fieldName)) {
//                Type propertyType = ReflectionUtil.GetPropertyType(context.rootType, fieldName);
//                ReflectionUtil.ObjectArray2[0] = context.rootType;
//                ReflectionUtil.ObjectArray2[1] = fieldName;
//                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                    typeof(AccessExpression_RootProperty<>),
//                    propertyType,
//                    ReflectionUtil.ObjectArray2
//                );
//            }
//
//            throw new Exception($"Cannot resolve {fieldName} as a field or property on type {context.rootType.Name}");
//        }
//
//        private Expression VisitParenNode(ParenExpressionNodeOld nodeOld) {
//            return ParenExpressionFactory.CreateParenExpression(Visit(nodeOld.expressionNodeOld));
//        }
//
//        private Expression VisitOperatorExpression(OperatorExpressionNodeOld nodeOld) {
//            Type leftType = nodeOld.left.GetYieldedType(context);
//            Type rightType = nodeOld.right.GetYieldedType(context);
//            object leftExpression = null;
//            object rightExpression = null;
//
//            switch (nodeOld.OpType) {
//                case OperatorType.Plus:
//
//                    if (leftType == typeof(string) || rightType == typeof(string)) {
//                        Type openType = typeof(OperatorExpression_StringConcat<,>);
//                        leftExpression = Visit(nodeOld.left);
//                        rightExpression = Visit(nodeOld.right);
//                        ReflectionUtil.TypeArray2[0] = leftType;
//                        ReflectionUtil.TypeArray2[1] = rightType;
//                        ReflectionUtil.ObjectArray2[0] = leftExpression;
//                        ReflectionUtil.ObjectArray2[1] = rightExpression;
//
//                        return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                            openType,
//                            ReflectionUtil.TypeArray2,
//                            ReflectionUtil.ObjectArray2
//                        );
//                    }
//
//                    if (ReflectionUtil.AreNumericTypesCompatible(leftType, rightType)) {
//                        return OperatorExpression_Arithmetic.Create(OperatorType.Plus, Visit(nodeOld.left), Visit(nodeOld.right));
//                    }
//
//                    break;
//
//                case OperatorType.Minus:
//                case OperatorType.Divide:
//                case OperatorType.Times:
//                case OperatorType.Mod:
//
//                    if (ReflectionUtil.AreNumericTypesCompatible(leftType, rightType)) {
//                        return OperatorExpression_Arithmetic.Create(nodeOld.OpType, Visit(nodeOld.left), Visit(nodeOld.right));
//                    }
//
//                    break;
//
//                case OperatorType.TernaryCondition:
//
//                    return VisitOperator_TernaryCondition(nodeOld);
//
//                case OperatorType.TernarySelection:
//                    throw new Exception("Should never visit a TernarySelection operator");
//
//                case OperatorType.GreaterThan:
//                case OperatorType.GreaterThanEqualTo:
//                case OperatorType.LessThan:
//                case OperatorType.LessThanEqualTo:
//                    return null;//new OperatorExpression_Comparison(nodeOld.OpType, Visit(nodeOld.left), Visit(nodeOld.right));
//
//                case OperatorType.Equals:
//                case OperatorType.NotEquals: {
//                    Type openEqualityType = typeof(OperatorExpression_Equality<,>);
//                    Type leftNodeType = nodeOld.left.GetYieldedType(context);
//                    Type rightNodeType = nodeOld.right.GetYieldedType(context);
//
//                    leftExpression = Visit(nodeOld.left);
//                    rightExpression = Visit(nodeOld.right);
//                    ReflectionUtil.ObjectArray3[0] = nodeOld.OpType;
//                    ReflectionUtil.ObjectArray3[1] = leftExpression;
//                    ReflectionUtil.ObjectArray3[2] = rightExpression;
//                    ReflectionUtil.TypeArray2[0] = leftNodeType;
//                    ReflectionUtil.TypeArray2[1] = rightNodeType;
//                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        openEqualityType,
//                        ReflectionUtil.TypeArray2,
//                        ReflectionUtil.ObjectArray3
//                    );
//                }
//                case OperatorType.And:
//                case OperatorType.Or: {
//                    Type leftNodeType = nodeOld.left.GetYieldedType(context);
//                    Type rightNodeType = nodeOld.right.GetYieldedType(context);
//                    if (leftNodeType == typeof(bool) && rightNodeType == typeof(bool)) {
//                        return new OperatorExpression_AndOrBool(nodeOld.OpType, (Expression<bool>) Visit(nodeOld.left), (Expression<bool>) Visit(nodeOld.right));
//                    }
//                    else if (leftNodeType.IsClass && rightNodeType.IsClass) {
//                        ReflectionUtil.ObjectArray3[0] = nodeOld.OpType;
//                        ReflectionUtil.ObjectArray3[1] = Visit(nodeOld.left);
//                        ReflectionUtil.ObjectArray3[2] = Visit(nodeOld.right);
//                        ReflectionUtil.TypeArray2[0] = leftNodeType;
//                        ReflectionUtil.TypeArray2[1] = rightNodeType;
//                        return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                            typeof(OperatorExpression_AndOrObject<,>),
//                            ReflectionUtil.TypeArray2,
//                            ReflectionUtil.ObjectArray3
//                        );
//                    }
//                    else if (leftNodeType.IsClass && rightNodeType == typeof(bool)) {
//                        ReflectionUtil.ObjectArray3[0] = nodeOld.OpType;
//                        ReflectionUtil.ObjectArray3[1] = Visit(nodeOld.left);
//                        ReflectionUtil.ObjectArray3[2] = Visit(nodeOld.right);
//                        ReflectionUtil.TypeArray1[0] = leftNodeType;
//                        return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                            typeof(OperatorExpression_AndOrObjectBool<>),
//                            ReflectionUtil.TypeArray1,
//                            ReflectionUtil.ObjectArray3
//                        );
//                    }
//                    else if (leftNodeType == typeof(bool) && rightNodeType.IsClass) {
//                        ReflectionUtil.ObjectArray3[0] = nodeOld.OpType;
//                        ReflectionUtil.ObjectArray3[1] = Visit(nodeOld.left);
//                        ReflectionUtil.ObjectArray3[2] = Visit(nodeOld.right);
//                        ReflectionUtil.TypeArray1[0] = rightNodeType;
//                        return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                            typeof(OperatorExpression_AndOrBoolObject<>),
//                            ReflectionUtil.TypeArray1,
//                            ReflectionUtil.ObjectArray3
//                        );
//                    }
//
//                    break;
//                }
//            }
//
//            throw new Exception("Bad operator expression");
//        }
//
//        private Expression VisitUnaryExpression(UnaryExpressionNodeOld nodeOld) {
//            Type yieldType = nodeOld.expression.GetYieldedType(context);
//            if (yieldType == typeof(bool)) {
//                if (nodeOld.op == OperatorType.Not) {
//                    return new UnaryExpression_Boolean((Expression<bool>) Visit(nodeOld.expression));
//                }
//
//                throw new Exception("Unary but not boolean operator");
//            }
//
//            if (yieldType == typeof(string)) {
//                if (nodeOld.op == OperatorType.Not) {
//                    return new UnaryExpression_StringBoolean((Expression<string>) Visit(nodeOld.expression));
//                }
//            }
//
//            if (yieldType.IsClass) {
//                if (nodeOld.op == OperatorType.Not) {
//                    return new UnaryExpression_ObjectBoolean((Expression<object>) Visit(nodeOld.expression));
//                }
//            }
//
//            if (!IsNumericType(yieldType)) {
//                // todo -- error here for struct case?
//                return null;
//            }
//
//            switch (nodeOld.op) {
//                case OperatorType.Plus:
//                    return UnaryExpression_PlusFactory.Create(Visit(nodeOld.expression));
//
//                case OperatorType.Minus:
//                    return UnaryExpression_MinusFactory.Create(Visit(nodeOld.expression));
//            }
//
//            return null;
//        }
//
//        private static bool IsNumericType(Type type) {
//            return type == typeof(int)
//                   || type == typeof(float)
//                   || type == typeof(double);
//        }
//
//        private Expression VisitAccessExpression(AccessExpressionNodeOld nodeOld) {
//            string contextName = nodeOld.identifierNodeOld.identifier;
//            Type headType;
//            object arg0 = contextName;
//            AccessExpressionType expressionType = AccessExpressionType.AliasLookup;
//            bool isStaticReferenceExpression = false;
//
//            if (contextName[0] == '$') {
//                for (int i = 0; i < expressionAliasResolvers.Count; i++) {
//                    if (contextName == expressionAliasResolvers[i].aliasName) {
//                        Expression retn = expressionAliasResolvers[i].CompileAsAccessExpression(context, nodeOld, Visit);
//                        if (retn == null) {
//                            throw new ParseException($"Resolver {expressionAliasResolvers[i]} failed to parse {contextName}");
//                        }
//
//                        return retn;
//                    }
//                }
//            }
//
//            if (nodeOld.identifierNodeOld is ExternalReferenceIdentifierNodeOld) {
//                throw new NotImplementedException();
////                isStaticReferenceExpression = true;
////                headType = (Type) context.ResolveConstAlias(contextName);
////                // todo -- this will break for non type references... its possible that we want a constant value or something else here
////                if (headType == null) {
////                    throw new Exception("Unable to resolve alias: " + contextName);
////                }
//            }
//            else {
//                headType = ReflectionUtil.ResolveFieldOrPropertyType(context.rootType, contextName);
//            }
//
//            if (headType == null) {
//                throw new Exception("Missing field or alias for access on context: " + contextName);
//            }
//
//            if (headType.IsPrimitive) {
//                throw new Exception(
//                    $"Attempting property access on type {headType.Name} on a primitive field {contextName}");
//            }
//
//            if (headType.IsEnum) {
//                if (nodeOld.parts.Count != 1) {
//                    throw new Exception("Trying to reference nested Enum value, which is not possible");
//                }
//
//                if (!(nodeOld.parts[0] is PropertyAccessExpressionPartNodeOld)) {
//                    throw new Exception("Trying to read enum value but encountered array access");
//                }
//
//                object val = Enum.Parse(headType, ((PropertyAccessExpressionPartNodeOld) nodeOld.parts[0]).fieldName);
//                ReflectionUtil.ObjectArray1[0] = val;
//                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(ConstantExpression<>), headType, ReflectionUtil.ObjectArray1);
//            }
//
//            bool isRootContext = !(nodeOld.identifierNodeOld is SpecialIdentifierNodeOld) && !(nodeOld.identifierNodeOld is ExternalReferenceIdentifierNodeOld);
//            int startOffset = isRootContext ? 1 : 0;
//            int partCount = nodeOld.parts.Count;
//            if (isRootContext) {
//                partCount = nodeOld.parts.Count + 1;
//            }
//            else if (isStaticReferenceExpression) {
//                partCount = nodeOld.parts.Count; //== 1 ? 1 : node.parts.Count - 1;// 1;
//            }
//            else {
//                partCount = nodeOld.parts.Count;
//            }
//
//            Type lastType = headType;
//            AccessExpressionPart[] parts = new AccessExpressionPart[partCount];
//
//            if (isStaticReferenceExpression) {
//                startOffset = 1;
//                PropertyAccessExpressionPartNodeOld propertyPart = nodeOld.parts[0] as PropertyAccessExpressionPartNodeOld;
//                if (propertyPart != null) {
//                    FieldInfo fieldInfo = ReflectionUtil.GetStaticFieldInfo(headType, propertyPart.fieldName);
//                    if (fieldInfo != null) {
//                        arg0 = fieldInfo;
//                        expressionType = AccessExpressionType.StaticField;
//                        lastType = fieldInfo.FieldType;
//                        parts[0] = new AccessExpressionPart_StaticField(fieldInfo);
//                    }
//                    else {
//                        PropertyInfo propertyInfo = ReflectionUtil.GetStaticPropertyInfo(headType, propertyPart.fieldName);
//                        expressionType = AccessExpressionType.StaticProperty;
//                        if (propertyInfo == null) {
//                            throw new Exception("Imported values must be static fields or properties. " +
//                                                $"Cannot find a static field or property called {propertyPart.fieldName} on type {headType}");
//                        }
//
//                        arg0 = propertyInfo;
//                        lastType = propertyInfo.PropertyType;
//                        parts[0] = new AccessExpressionPart_StaticProperty(propertyInfo);
//                    }
//                }
//
//                for (int i = startOffset; i < partCount; i++) {
//                    AccessExpressionPartNodeOld part = nodeOld.parts[i];
//                    propertyPart = part as PropertyAccessExpressionPartNodeOld;
//                    if (propertyPart != null) {
//                        string fieldName = propertyPart.fieldName;
//                        FieldInfo fieldInfo = ReflectionUtil.GetInstanceOrStaticFieldInfo(lastType, fieldName);
//                        if (fieldInfo != null) {
//                            lastType = fieldInfo.FieldType;
//                            parts[i] = new AccessExpressionPart_Field(fieldName);
//                        }
//                        else {
//                            PropertyInfo propertyInfo = ReflectionUtil.GetInstanceOrStaticPropertyInfo(lastType, fieldName);
//
//                            if (propertyInfo == null) {
//                                throw new UIForia.ParseException($"Unable to find field with name '{fieldName}' on type {lastType}");
//                            }
//
//                            lastType = propertyInfo.PropertyType;
//                            if (ReflectionUtil.IsPropertyStatic(propertyInfo)) {
//                                parts[i] = new AccessExpressionPart_StaticProperty(propertyInfo);
//                            }
//                            else {
//                                parts[i] = new AccessExpressionPart_Property(fieldName);
//                            }
//                        }
//
//                        continue;
//                    }
//
//                    ArrayAccessExpressionNodeOld arrayPart = part as ArrayAccessExpressionNodeOld;
//                    if (arrayPart != null) {
//                        Expression<int> indexExpression = (Expression<int>) Visit(arrayPart.expressionNodeOld);
//                        lastType = ReflectionUtil.GetArrayElementTypeOrThrow(lastType);
//                        parts[i] = new AccessExpressionPart_List(indexExpression);
//                        continue;
//                    }
//
//                    throw new Exception("Unknown AccessExpression Type: " + part.GetType());
//                }
//            }
//            else {
//                for (int i = startOffset; i < partCount; i++) {
//                    AccessExpressionPartNodeOld part = nodeOld.parts[i - startOffset];
//
//                    PropertyAccessExpressionPartNodeOld propertyPart = part as PropertyAccessExpressionPartNodeOld;
//
//                    if (propertyPart != null) {
//                        string fieldName = propertyPart.fieldName;
//                        FieldInfo fieldInfo = ReflectionUtil.GetInstanceOrStaticFieldInfo(lastType, fieldName);
//                        if (fieldInfo != null) {
//                            lastType = fieldInfo.FieldType;
//                            parts[i] = new AccessExpressionPart_Field(fieldName);
//                        }
//                        else {
//                            PropertyInfo propertyInfo = ReflectionUtil.GetInstanceOrStaticPropertyInfo(lastType, fieldName);
//                            if (propertyInfo == null) {
//                                throw new UIForia.ParseException($"Unable to find field or property with name '{fieldName}' on type {lastType}");
//                            }
//
//                            lastType = propertyInfo.PropertyType;
//                            if (ReflectionUtil.IsPropertyStatic(propertyInfo)) {
//                                parts[i] = new AccessExpressionPart_StaticProperty(propertyInfo);
//                            }
//                            else {
//                                parts[i] = new AccessExpressionPart_Property(fieldName);
//                            }
//                        }
//
//                        continue;
//                    }
//
//                    ArrayAccessExpressionNodeOld arrayPart = part as ArrayAccessExpressionNodeOld;
//                    if (arrayPart != null) {
//                        Expression<int> indexExpression = (Expression<int>) Visit(arrayPart.expressionNodeOld);
//                        lastType = ReflectionUtil.GetArrayElementTypeOrThrow(lastType);
//                        parts[i] = new AccessExpressionPart_List(indexExpression);
//                        continue;
//                    }
//
//                    MethodAccessExpressionPartNodeOld methodPart = part as MethodAccessExpressionPartNodeOld;
//                    if (methodPart != null) {
//                        // todo only supports Action and Func right now, not actual methods
//
//                        Type methodType = null;
//                        Type targetType = headType;
//                        AccessExpressionPart prev = parts[i - 1];
//
//                        // todo -- use the array element type as the action/func type 
//                        // todo -- this only supports actions right that have names
//                        // todo -- we only support 1 level of dot right now, need to get type from parts list - 2 || head
//
//                        if (prev is AccessExpressionPart_List) {
//                            throw new Exception("Array method access in expressions is not yet supported");
//                        }
//
//                        if (prev is AccessExpressionPart_Field) {
//                            string fieldName = (prev as AccessExpressionPart_Field).fieldName;
//                            methodType = ReflectionUtil.GetFieldType(targetType, fieldName);
//                        }
//                        else if (prev is AccessExpressionPart_Property) {
//                            string propertyName = (prev as AccessExpressionPart_Property).propertyName;
//                            methodType = ReflectionUtil.GetPropertyType(targetType, propertyName);
//                        }
//
//                        if (methodType == null) {
//                            // todo fix this terrible error message
//                            throw new Exception("Error parsing method access expression");
//                        }
//
//                        if (ReflectionUtil.IsAction(methodType)) {
//                            parts[i] = CreateActionAccessPart(methodType, methodPart.signatureNodeOld);
//                            lastType = typeof(Terminal);
//                            if (i != partCount - 1) {
//                                throw new Exception("Encountered void return type but access chain continues");
//                            }
//
//                            break;
//                        }
//                        else {
//                            AccessExpressionPart_Func fn = CreateFuncAccessPart(methodType, methodPart.signatureNodeOld);
//                            parts[i] = fn;
//                            lastType = fn.RetnType;
//                        }
//
//                        continue;
//                    }
//
//                    throw new Exception("Unknown AccessExpression Type: " + part.GetType());
//                }
//            }
//
//            if (isRootContext) {
//                parts[0] = new AccessExpressionPart_Field(contextName);
//            }
//
//            switch (expressionType) {
//                case AccessExpressionType.AliasLookup:
//                case AccessExpressionType.RootLookup:
//                    ReflectionUtil.ObjectArray2[0] = arg0;
//                    ReflectionUtil.ObjectArray2[1] = parts;
//                    ReflectionUtil.TypeArray2[0] = lastType;
//                    ReflectionUtil.TypeArray2[1] = isRootContext ? context.rootType : headType;
//                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpression<,>),
//                        ReflectionUtil.TypeArray2,
//                        ReflectionUtil.ObjectArray2
//                    );
//
//                case AccessExpressionType.StaticField:
//                    ReflectionUtil.ObjectArray2[0] = arg0;
//                    ReflectionUtil.ObjectArray2[1] = parts;
//                    ReflectionUtil.TypeArray1[0] = lastType;
//                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionStaticField<>),
//                        ReflectionUtil.TypeArray1,
//                        ReflectionUtil.ObjectArray2
//                    );
//
//                case AccessExpressionType.StaticProperty:
//                    ReflectionUtil.ObjectArray2[0] = arg0;
//                    ReflectionUtil.ObjectArray2[1] = parts;
//                    ReflectionUtil.TypeArray1[0] = lastType;
//                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionStaticProperty<>),
//                        ReflectionUtil.TypeArray1,
//                        ReflectionUtil.ObjectArray2
//                    );
//                case AccessExpressionType.Constant:
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }
//
//        private AccessExpressionPart_Func CreateFuncAccessPart(Type type, MethodSignatureNodeOld signatureNodeOld) {
//            Type[] argTypes = type.GetGenericArguments();
//            if (argTypes.Length == 1) {
//                return (AccessExpressionPart_Func) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                    typeof(AccessExpressionPart_Func<>),
//                    argTypes,
//                    ReflectionUtil.ObjectArray0
//                );
//            }
//
//            Expression[] expressions = new Expression[signatureNodeOld.parts.Count];
//
//            for (int i = 1; i < argTypes.Length; i++) {
//                Type requiredType = argTypes[i];
//                ExpressionNodeOld argumentNodeOld = signatureNodeOld.parts[i - 1];
//                Expression argumentExpression = Visit(argumentNodeOld);
//                expressions[i - 1] = HandleCasting(requiredType, argumentExpression);
//                if (!requiredType.IsAssignableFrom(expressions[i - 1].YieldedType)) {
//                    throw new Exception($"Cannot use parameter of type {expressions[i - 1].YieldedType} for parameter of type {requiredType}");
//                }
//            }
//
//            switch (argTypes.Length) {
//                case 2:
//                    ReflectionUtil.ObjectArray1[0] = expressions[0];
//                    return (AccessExpressionPart_Func) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionPart_Func<,>),
//                        argTypes,
//                        ReflectionUtil.ObjectArray1
//                    );
//                case 3:
//                    ReflectionUtil.ObjectArray2[0] = expressions[0];
//                    ReflectionUtil.ObjectArray2[1] = expressions[1];
//                    return (AccessExpressionPart_Func) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionPart_Func<,,>),
//                        argTypes,
//                        ReflectionUtil.ObjectArray2
//                    );
//                case 4:
//                    ReflectionUtil.ObjectArray3[0] = expressions[0];
//                    ReflectionUtil.ObjectArray3[1] = expressions[1];
//                    ReflectionUtil.ObjectArray3[2] = expressions[2];
//                    return (AccessExpressionPart_Func) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionPart_Func<,,,>),
//                        argTypes,
//                        ReflectionUtil.ObjectArray3
//                    );
//                case 5:
//                    ReflectionUtil.ObjectArray4[0] = expressions[0];
//                    ReflectionUtil.ObjectArray4[1] = expressions[1];
//                    ReflectionUtil.ObjectArray4[2] = expressions[2];
//                    ReflectionUtil.ObjectArray4[3] = expressions[3];
//                    return (AccessExpressionPart_Func) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionPart_Func<,,,,>),
//                        argTypes,
//                        ReflectionUtil.ObjectArray4
//                    );
//                default:
//                    throw new Exception("Expression only support Actions with 4 arguments");
//            }
//        }
//
//        private AccessExpressionPart_Method CreateActionAccessPart(Type type, MethodSignatureNodeOld signatureNodeOld) {
//            Type[] argTypes = type.GetGenericArguments();
//            if (argTypes.Length == 0) {
//                return new AccessExpressionPart_Method();
//            }
//
//            Expression[] expressions = new Expression[signatureNodeOld.parts.Count];
//
//            for (int i = 0; i < argTypes.Length; i++) {
//                Type requiredType = argTypes[i];
//                ExpressionNodeOld argumentNodeOld = signatureNodeOld.parts[i];
//                Expression argumentExpression = Visit(argumentNodeOld);
//                expressions[i] = HandleCasting(requiredType, argumentExpression);
//            }
//
//            ValidateParameterTypes(argTypes, expressions);
//
//            switch (argTypes.Length) {
//                case 1:
//                    ReflectionUtil.ObjectArray1[0] = expressions[0];
//                    return (AccessExpressionPart_Method) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionPart_Method<>),
//                        argTypes,
//                        ReflectionUtil.ObjectArray1
//                    );
//                case 2:
//                    ReflectionUtil.ObjectArray2[0] = expressions[0];
//                    ReflectionUtil.ObjectArray2[1] = expressions[1];
//                    return (AccessExpressionPart_Method) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionPart_Method<,>),
//                        argTypes,
//                        ReflectionUtil.ObjectArray2
//                    );
//                case 3:
//                    ReflectionUtil.ObjectArray3[0] = expressions[0];
//                    ReflectionUtil.ObjectArray3[1] = expressions[1];
//                    ReflectionUtil.ObjectArray3[2] = expressions[2];
//                    return (AccessExpressionPart_Method) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionPart_Method<,,>),
//                        argTypes,
//                        ReflectionUtil.ObjectArray3
//                    );
//                case 4:
//                    ReflectionUtil.ObjectArray4[0] = expressions[0];
//                    ReflectionUtil.ObjectArray4[1] = expressions[1];
//                    ReflectionUtil.ObjectArray4[2] = expressions[2];
//                    ReflectionUtil.ObjectArray4[3] = expressions[3];
//                    return (AccessExpressionPart_Method) ReflectionUtil.CreateGenericInstanceFromOpenType(
//                        typeof(AccessExpressionPart_Method<,,,>),
//                        argTypes,
//                        ReflectionUtil.ObjectArray4
//                    );
//                default:
//                    throw new Exception("Expression only support Actions with 4 arguments");
//            }
//        }
//
//        private static Expression VisitConstant(LiteralValueNodeOld nodeOld) {
//            if (nodeOld is NumericLiteralNodeOld) {
//                return VisitNumericLiteralNode((NumericLiteralNodeOld) nodeOld);
//            }
//
//            if (nodeOld is BooleanLiteralNodeOld) {
//                return new ConstantExpression<bool>(((BooleanLiteralNodeOld) nodeOld).value);
//            }
//
//            if (nodeOld is StringLiteralNodeOld) {
//                return new ConstantExpression<string>(((StringLiteralNodeOld) nodeOld).value);
//            }
//
//            return null;
//        }
//
//        private static Expression VisitNumericLiteralNode(NumericLiteralNodeOld nodeOld) {
//            if (nodeOld is FloatLiteralNodeOld literalNode) {
//                return new ConstantExpression<float>(literalNode.value);
//            }
//
//            if (nodeOld is IntLiteralNodeOld intLiteralNode) {
//                return new ConstantExpression<int>(intLiteralNode.value);
//            }
//
//            return new ConstantExpression<double>(((DoubleLiteralNodeOld) nodeOld).value);
//        }
//
//        private Expression VisitOperator_TernaryCondition(OperatorExpressionNodeOld nodeOld) {
//            Expression<bool> condition = (Expression<bool>) Visit(nodeOld.left);
//            OperatorExpressionNodeOld select = (OperatorExpressionNodeOld) nodeOld.right;
//
//            if (select.OpType != OperatorType.TernarySelection) {
//                throw new Exception("Bad ternary");
//            }
//
//            Expression right = Visit(select.right);
//            Expression left = Visit(select.left);
//
//            // todo -- need to assert a type match here
//            Type commonBase = ReflectionUtil.GetCommonBaseClass(right.YieldedType, left.YieldedType);
//
//            if (commonBase == null || commonBase == typeof(ValueType) || commonBase == typeof(object)) {
//                throw new Exception(
//                    $"Types in ternary don't match: {right.YieldedType.Name} is not {left.YieldedType.Name}");
//            }
//
//            if (commonBase == typeof(int)) {
//                return new OperatorExpression_Ternary<int>(
//                    condition,
//                    (Expression<int>) left,
//                    (Expression<int>) right
//                );
//            }
//
//            if (commonBase == typeof(float)) {
//                return new OperatorExpression_Ternary<float>(
//                    condition,
//                    (Expression<float>) left,
//                    (Expression<float>) right
//                );
//            }
//
//            if (commonBase == typeof(double)) {
//                return new OperatorExpression_Ternary<double>(
//                    condition,
//                    (Expression<double>) left,
//                    (Expression<double>) right
//                );
//            }
//
//            if (commonBase == typeof(string)) {
//                return new OperatorExpression_Ternary<string>(
//                    condition,
//                    (Expression<string>) left,
//                    (Expression<string>) right
//                );
//            }
//
//            if (commonBase == typeof(bool)) {
//                return new OperatorExpression_Ternary<bool>(
//                    condition,
//                    (Expression<bool>) left,
//                    (Expression<bool>) right
//                );
//            }
//
//            Type openType = typeof(OperatorExpression_Ternary<>);
//            ReflectionUtil.ObjectArray3[0] = condition;
//            ReflectionUtil.ObjectArray3[1] = left;
//            ReflectionUtil.ObjectArray3[2] = right;
//            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(openType, commonBase, ReflectionUtil.ObjectArray3);
//        }
//
//        [Flags]
//        private enum MethodType {
//
//            Static = 1 << 0,
//            Instance = 1 << 1,
//            Void = 1 << 2,
//
//            InstanceVoid = Instance | Void,
//            StaticVoid = Static | Void
//
//        }
//
//        private enum AccessExpressionType {
//
//            Constant,
//            RootLookup,
//            StaticField,
//            StaticProperty,
//            AliasLookup
//
//        }
//
//    }
//
//}