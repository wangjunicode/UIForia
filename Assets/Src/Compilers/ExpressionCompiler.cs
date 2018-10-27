using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Src.Compilers.CastHandlers;
using UnityEngine;

namespace Src {

    // takes the place of void since generics can't use void types
    public sealed class Terminal { }

    public class ExpressionCompiler {

        public ContextDefinition context;
        private List<ICastHandler> userCastHandlers;

        private static readonly ExpressionParser parser = new ExpressionParser();

        private static readonly List<ICastHandler> builtInCastHandlers = new List<ICastHandler>() {
            new CastHandler_ToString(),
            new CastHandler_ColorToVector4(),
            new CastHandler_DoubleToMeasurement(),
            new CastHandler_FloatToInt(),
            new CastHandler_FloatToMeasurement(),
            new CastHandler_IntToDouble(),
            new CastHandler_IntToFloat(),
            new CastHandler_IntToMeasurement(),
            new CastHandler_FloatToFixedLength(),
            new CastHandler_DoubleToFixedLength(),
            new CastHandler_IntToFixedLength(),
            new CastHandler_Vector2ToVector3(),
            new CastHandler_Vector3ToVector2(),
            new CastHandler_Vector4ToColor()
        };

        [PublicAPI]
        public ExpressionCompiler(ContextDefinition context) {
            this.context = context;
        }

        [PublicAPI]
        public Expression Compile(ExpressionNode root) {
            return Visit(root);
        }

        [PublicAPI]
        public Expression<T> Compile<T>(ExpressionNode root) {
            return (Expression<T>) HandleCasting(typeof(T), Visit(root));
        }

        [PublicAPI]
        public Expression Compile(string source) {
            try {
                return Visit(parser.Parse(source));
            }
            catch (Exception e) {
                Debug.Log("Error compiling: " + source);
                Debug.Log(e.StackTrace);
                throw e;
            }
        }

        [PublicAPI]
        public Expression Compile(Type outputType, string source) {
            return HandleCasting(outputType, Visit(parser.Parse(source)));
        }

        [PublicAPI]
        public Expression<T> Compile<T>(string source) {
            return (Expression<T>) HandleCasting(typeof(T), Visit(parser.Parse(source)));
        }

        [PublicAPI]
        public void AddCastHandler(ICastHandler handler) {
            if (userCastHandlers == null) {
                userCastHandlers = new List<ICastHandler>();
            }

            userCastHandlers.Add(handler);
        }

        [PublicAPI]
        public void RemoveCastHandler(ICastHandler handler) {
            userCastHandlers?.Remove(handler);
        }

        [PublicAPI]
        public void SetContext(ContextDefinition contextDefinition) {
            this.context = contextDefinition;
        }

        [PublicAPI]
        public void AddRuntimeAlias(string aliasName, Type aliasType) {
            context.AddRuntimeAlias(aliasName, aliasType);
        }

        [PublicAPI]
        public void RemoveRuntimeAlias(string aliasName) {
            context.RemoveRuntimeAlias(aliasName);
        }

        private Expression Visit(ExpressionNode node) {
            switch (node.expressionType) {
                case ExpressionNodeType.AliasAccessor:
                    return VisitAliasNode((AliasExpressionNode) node);

                case ExpressionNodeType.Paren:
                    return VisitParenNode((ParenExpressionNode) node);

                case ExpressionNodeType.RootContextAccessor:
                    return VisitRootContextAccessor((RootContextLookupNode) node);

                case ExpressionNodeType.LiteralValue:
                    return VisitConstant((LiteralValueNode) node);

                case ExpressionNodeType.Accessor:
                    return VisitAccessExpression((AccessExpressionNode) node);

                case ExpressionNodeType.Unary:
                    return VisitUnaryExpression((UnaryExpressionNode) node);

                case ExpressionNodeType.Operator:
                    return VisitOperatorExpression((OperatorExpressionNode) node);

                case ExpressionNodeType.MethodCall:
                    return VisitMethodCallExpression((MethodCallNode) node);
            }

            return null;
        }

        private Expression HandleCasting(Type requiredType, Expression expression) {
            Type yieldedType = expression.YieldedType;

            if (yieldedType == requiredType) {
                return expression;
            }

            if (userCastHandlers != null) {
                for (int i = 0; i < userCastHandlers.Count; i++) {
                    if (userCastHandlers[i].CanHandle(requiredType, yieldedType)) {
                        return userCastHandlers[i].Cast(requiredType, expression);
                    }
                }
            }

            for (int i = 0; i < builtInCastHandlers.Count; i++) {
                if (builtInCastHandlers[i].CanHandle(requiredType, yieldedType)) {
                    return builtInCastHandlers[i].Cast(requiredType, expression);
                }
            }

            return expression;
        }

        private static void ValidateParameterTypes(ParameterInfo[] parameters, Expression[] arguments) {
            for (int i = 0; i < parameters.Length; i++) {
                if (!parameters[i].ParameterType.IsAssignableFrom(arguments[i].YieldedType)) {
                    throw new Exception($"Cannot use parameter of type {arguments[i].YieldedType} for parameter of type {parameters[i].ParameterType}");
                }
            }
        }

        private Expression VisitMethodCallExpression(MethodCallNode node) {
            string methodName = node.identifierNode.identifier;

            Type[] yieldedTypes = node.signatureNode.parts.Select(p => p.GetYieldedType(context)).ToArray();
            MethodInfo info = (MethodInfo) context.ResolveConstAlias(methodName, yieldedTypes);

            info = info ?? ReflectionUtil.GetMethodInfo(context.rootType, methodName);

            if (info == null) {
                throw new Exception($"Cannot find method {methodName} on type {context.rootType.Name} or any registered aliases");
            }

            MethodType methodType = 0;
            bool isVoid = info.ReturnType == typeof(void);

            if (info.IsStatic) {
                methodType |= MethodType.Static;
            }
            else {
                methodType |= MethodType.Instance;
            }

            if (isVoid) {
                methodType |= MethodType.Void;
            }

            ParameterInfo[] parameters = info.GetParameters();
            IReadOnlyList<ExpressionNode> signatureParts = node.signatureNode.parts;

            if (parameters.Length != signatureParts.Count) {
                throw new Exception("Argument count is wrong for method "
                                    + methodName + " expected: " + parameters.Length
                                    + " but was provided: " + signatureParts.Count);
            }

            int genericOffset = 0;
            int extraArgumentCount = 2;

            if ((methodType & MethodType.Void) != 0) {
                extraArgumentCount--;
            }

            if ((methodType & MethodType.Static) != 0) {
                extraArgumentCount--;
            }

            Expression[] args = new Expression[signatureParts.Count];
            Type[] genericArguments = new Type[signatureParts.Count + extraArgumentCount];

            if ((methodType & MethodType.Instance) != 0) {
                genericArguments[0] = context.rootType; // todo -- this means root functions only, no chaining right now! 
                genericOffset = 1;
            }

            for (int i = 0; i < args.Length; i++) {
                Type requiredType = parameters[i].ParameterType;
                ExpressionNode argumentNode = signatureParts[i];
                Expression argumentExpression = Visit(argumentNode);
                args[i] = HandleCasting(requiredType, argumentExpression);

                genericArguments[i + genericOffset] = args[i].YieldedType;
            }

            if ((methodType & MethodType.Void) == 0) {
                genericArguments[genericArguments.Length - 1] = info.ReturnType;
            }

            ValidateParameterTypes(parameters, args);

            Type callType = GetMethodCallType(methodName, args.Length, genericArguments, methodType);

            ReflectionUtil.ObjectArray2[0] = info;
            ReflectionUtil.ObjectArray2[1] = args;

            return (Expression) ReflectionUtil.CreateGenericInstance(callType, ReflectionUtil.ObjectArray2);
        }

        [Flags]
        private enum MethodType {

            Static = 1 << 0,
            Instance = 1 << 1,
            Void = 1 << 2,

            InstanceVoid = Instance | Void,
            StaticVoid = Static | Void

        }

        private static Type GetMethodCallType(string methodName, int argumentCount, Type[] genericArguments, MethodType methodType) {
            switch (argumentCount) {
                case 0:

                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<>), genericArguments);
                        case MethodType.StaticVoid:
                            return typeof(MethodCallExpression_StaticVoid);
                        case MethodType.InstanceVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<>), genericArguments);
                        case MethodType.Instance:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,>), genericArguments);
                        default:
                            throw new Exception("Cannot create method");
                    }
                case 1:
                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,>), genericArguments);
                        case MethodType.StaticVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<>), genericArguments);
                        case MethodType.InstanceVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,>), genericArguments);
                        case MethodType.Instance:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,>), genericArguments);
                        default:
                            throw new Exception("Cannot create method");
                    }
                case 2:
                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,>), genericArguments);
                        case MethodType.StaticVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,>), genericArguments);
                        case MethodType.InstanceVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,>), genericArguments);
                        case MethodType.Instance:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,>), genericArguments);
                        default:
                            throw new Exception("Cannot create method");
                    }
                case 3:
                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,>), genericArguments);
                        case MethodType.StaticVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,,>), genericArguments);
                        case MethodType.InstanceVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,,>), genericArguments);
                        case MethodType.Instance:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,,>), genericArguments);
                        default:
                            throw new Exception("Cannot create method");
                    }

                case 4:
                    switch (methodType) {
                        case MethodType.Static:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,,>), genericArguments);
                        case MethodType.StaticVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_StaticVoid<,,,>), genericArguments);
                        case MethodType.InstanceVoid:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_InstanceVoid<,,,,>), genericArguments);
                        case MethodType.Instance:
                            return ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,,,>), genericArguments);
                        default:
                            throw new Exception("Cannot create method");
                    }

                default:
                    throw new Exception(
                        $"Expressions only support functions with up to 4 arguments. {methodName} is supplying {argumentCount} ");
            }
        }

        private Expression VisitAliasNode(AliasExpressionNode node) {
            Type aliasedType = context.ResolveRuntimeAliasType(node.alias);

            if (aliasedType == null) {
                throw new Exception("Unable to resolve alias: " + node.alias);
            }

            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(ResolveExpression_Alias<>),
                aliasedType,
                node.alias
            );
        }

        private Expression VisitRootContextAccessor(RootContextLookupNode node) {
            string fieldName = node.idNode.identifier;
            // todo -- alias is resolved before field access, might be an issue
            object constantAlias = context.ResolveConstAlias(fieldName);
            if (constantAlias != null) {
                Type aliasType = constantAlias.GetType();

                ReflectionUtil.ObjectArray1[0] = constantAlias;
                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(ConstantExpression<>),
                    aliasType,
                    ReflectionUtil.ObjectArray1
                );
            }

            if (ReflectionUtil.IsField(context.rootType, fieldName)) {
                Type fieldType = ReflectionUtil.GetFieldType(context.rootType, fieldName);
                ReflectionUtil.ObjectArray2[0] = context.rootType;
                ReflectionUtil.ObjectArray2[1] = fieldName;
                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(AccessExpression_RootField<>),
                    fieldType,
                    ReflectionUtil.ObjectArray2
                );
            }

            if (ReflectionUtil.IsProperty(context.rootType, fieldName)) {
                Type propertyType = ReflectionUtil.GetPropertyType(context.rootType, fieldName);
                ReflectionUtil.ObjectArray2[0] = context.rootType;
                ReflectionUtil.ObjectArray2[1] = fieldName;
                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(AccessExpression_RootProperty<>),
                    propertyType,
                    ReflectionUtil.ObjectArray2
                );
            }

            throw new Exception($"Cannot resolve {fieldName} as a field or property on type {context.rootType.Name}");
        }

        private Expression VisitParenNode(ParenExpressionNode node) {
            return ParenExpressionFactory.CreateParenExpression(Visit(node.expressionNode));
        }

        private Expression VisitOperatorExpression(OperatorExpressionNode node) {
            Type leftType = node.left.GetYieldedType(context);
            Type rightType = node.right.GetYieldedType(context);
            object leftExpression = null;
            object rightExpression = null;

            switch (node.OpType) {
                case OperatorType.Plus:

                    if (leftType == typeof(string) || rightType == typeof(string)) {
                        Type openType = typeof(OperatorExpression_StringConcat<,>);
                        ReflectionUtil.TypeArray2[0] = leftType;
                        ReflectionUtil.TypeArray2[1] = rightType;
                        leftExpression = Visit(node.left);
                        rightExpression = Visit(node.right);
                        ReflectionUtil.ObjectArray2[0] = leftExpression;
                        ReflectionUtil.ObjectArray2[1] = rightExpression;

                        return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            openType,
                            ReflectionUtil.TypeArray2,
                            ReflectionUtil.ObjectArray2
                        );
                    }

                    if (ReflectionUtil.AreNumericTypesCompatible(leftType, rightType)) {
                        return OperatorExpression_Arithmetic.Create(OperatorType.Plus, Visit(node.left),
                            Visit(node.right));
                    }

                    break;

                case OperatorType.Minus:
                case OperatorType.Divide:
                case OperatorType.Times:
                case OperatorType.Mod:

                    if (ReflectionUtil.AreNumericTypesCompatible(leftType, rightType)) {
                        return OperatorExpression_Arithmetic.Create(node.OpType, Visit(node.left), Visit(node.right));
                    }

                    break;

                case OperatorType.TernaryCondition:

                    return VisitOperator_TernaryCondition(node);

                case OperatorType.TernarySelection:
                    throw new Exception("Should never visit a TernarySelection operator");

                case OperatorType.GreaterThan:
                case OperatorType.GreaterThanEqualTo:
                case OperatorType.LessThan:
                case OperatorType.LessThanEqualTo:
                    return new OperatorExpression_Comparison(node.OpType, Visit(node.left), Visit(node.right));

                case OperatorType.Equals:
                case OperatorType.NotEquals:

                    Type openEqualityType = typeof(OperatorExpression_Equality<,>);
                    ReflectionUtil.TypeArray2[0] = node.left.GetYieldedType(context);
                    ReflectionUtil.TypeArray2[1] = node.right.GetYieldedType(context);

                    leftExpression = Visit(node.left);
                    rightExpression = Visit(node.right);
                    ReflectionUtil.ObjectArray3[0] = node.OpType;
                    ReflectionUtil.ObjectArray3[1] = leftExpression;
                    ReflectionUtil.ObjectArray3[2] = rightExpression;

                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        openEqualityType,
                        ReflectionUtil.TypeArray2,
                        ReflectionUtil.ObjectArray3
                    );
            }

            throw new Exception("Bad operator expression");
        }

        private Expression VisitUnaryExpression(UnaryExpressionNode node) {
            Type yieldType = node.expression.GetYieldedType(context);
            if (yieldType == typeof(bool)) {
                if (node.op == OperatorType.Not) {
                    return new UnaryExpression_Boolean((Expression<bool>) Visit(node.expression));
                }

                throw new Exception("Unary but not boolean operator");
            }

            if (!IsNumericType(yieldType)) {
                return null;
            }

            switch (node.op) {
                case OperatorType.Plus:
                    return UnaryExpression_PlusFactory.Create(Visit(node.expression));

                case OperatorType.Minus:
                    return UnaryExpression_MinusFactory.Create(Visit(node.expression));
            }

            return null;
        }

        private static bool IsNumericType(Type type) {
            return type == typeof(int)
                   || type == typeof(float)
                   || type == typeof(double);
        }

        private enum AccessExpressionType {

            Constant,
            RootLookup,
            StaticField,
            StaticProperty,

            AliasLookup

        }

        private Expression VisitAccessExpression(AccessExpressionNode node) {
            string contextName = node.identifierNode.identifier;
            Type headType;
            object arg0 = contextName;
            AccessExpressionType expressionType = AccessExpressionType.AliasLookup;
            bool isStaticReferenceExpression = false;

            if (node.identifierNode is ExternalReferenceIdentifierNode) {
                isStaticReferenceExpression = true;
                headType = (Type) context.ResolveConstAlias(contextName);
                // todo -- this will break for non type references... its possible that we want a constant value or something else here
                if (headType == null) {
                    throw new Exception("Unable to resolve alias: " + contextName);
                }
            }
            else {
                headType = context.ResolveRuntimeAliasType(contextName);
            }

            if (headType == null) {
                throw new Exception("Missing field or alias for access on context: " + contextName);
            }

            if (headType.IsPrimitive) {
                throw new Exception(
                    $"Attempting property access on type {headType.Name} on a primitive field {contextName}");
            }

            if (headType.IsEnum) {
                if (node.parts.Count != 1) {
                    throw new Exception("Trying to reference nested Enum value, which is not possible");
                }

                if (!(node.parts[0] is PropertyAccessExpressionPartNode)) {
                    throw new Exception("Trying to read enum value but encountered array access");
                }

                object val = Enum.Parse(headType, ((PropertyAccessExpressionPartNode) node.parts[0]).fieldName);
                ReflectionUtil.ObjectArray1[0] = val;
                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(ConstantExpression<>), headType, ReflectionUtil.ObjectArray1);
            }

            bool isRootContext = !(node.identifierNode is SpecialIdentifierNode) && !(node.identifierNode is ExternalReferenceIdentifierNode);
            int startOffset = isRootContext ? 1 : 0;
            int partCount = node.parts.Count;
            if (isRootContext) {
                partCount = node.parts.Count + 1;
            }
            else if (isStaticReferenceExpression) {
                partCount = node.parts.Count; //== 1 ? 1 : node.parts.Count - 1;// 1;
            }
            else {
                partCount = node.parts.Count;
            }

            Type lastType = headType;
            AccessExpressionPart[] parts = new AccessExpressionPart[partCount];

            if (isStaticReferenceExpression) {
                startOffset = 1;
                PropertyAccessExpressionPartNode propertyPart = node.parts[0] as PropertyAccessExpressionPartNode;
                if (propertyPart != null) {
                    FieldInfo fieldInfo = ReflectionUtil.GetStaticFieldInfo(headType, propertyPart.fieldName);
                    if (fieldInfo != null) {
                        arg0 = fieldInfo;
                        expressionType = AccessExpressionType.StaticField;
                        lastType = fieldInfo.FieldType;
                        parts[0] = new AccessExpressionPart_StaticField(fieldInfo);
                    }
                    else {
                        PropertyInfo propertyInfo = ReflectionUtil.GetStaticPropertyInfo(headType, propertyPart.fieldName);
                        expressionType = AccessExpressionType.StaticProperty;
                        if (propertyInfo == null) {
                            throw new Exception("Imported values must be static fields or properties. " +
                                                $"Cannot find a static field or property called {propertyPart.fieldName} on type {headType}");
                        }

                        arg0 = propertyInfo;
                        lastType = propertyInfo.PropertyType;
                        parts[0] = new AccessExpressionPart_StaticProperty(propertyInfo);
                    }
                }

                for (int i = startOffset; i < partCount; i++) {
                    AccessExpressionPartNode part = node.parts[i];
                    propertyPart = part as PropertyAccessExpressionPartNode;
                    if (propertyPart != null) {
                        string fieldName = propertyPart.fieldName;
                        FieldInfo fieldInfo = ReflectionUtil.GetInstanceOrStaticFieldInfo(lastType, fieldName);
                        if (fieldInfo != null) {
                            lastType = fieldInfo.FieldType;
                            parts[i] = new AccessExpressionPart_Field(fieldName);
                        }
                        else {
                            PropertyInfo propertyInfo = ReflectionUtil.GetInstanceOrStaticPropertyInfo(lastType, fieldName);
                            lastType = propertyInfo.PropertyType;
                            if (ReflectionUtil.IsPropertyStatic(propertyInfo)) {
                                parts[i] = new AccessExpressionPart_StaticProperty(propertyInfo);
                            }
                            else {
                                parts[i] = new AccessExpressionPart_Property(fieldName);
                            }
                        }

                        continue;
                    }

                    ArrayAccessExpressionNode arrayPart = part as ArrayAccessExpressionNode;
                    if (arrayPart != null) {
                        Expression<int> indexExpression = (Expression<int>) Visit(arrayPart.expressionNode);
                        lastType = ReflectionUtil.GetArrayElementTypeOrThrow(lastType);
                        parts[i] = new AccessExpressionPart_List(indexExpression);
                        continue;
                    }

                    throw new Exception("Unknown AccessExpression Type: " + part.GetType());
                }
            }
            else {
                for (int i = startOffset; i < partCount; i++) {
                    AccessExpressionPartNode part = node.parts[i - startOffset];

                    PropertyAccessExpressionPartNode propertyPart = part as PropertyAccessExpressionPartNode;

                    if (propertyPart != null) {
                        string fieldName = propertyPart.fieldName;
                        FieldInfo fieldInfo = ReflectionUtil.GetInstanceOrStaticFieldInfo(lastType, fieldName);
                        if (fieldInfo != null) {
                            lastType = fieldInfo.FieldType;
                            parts[i] = new AccessExpressionPart_Field(fieldName);
                        }
                        else {
                            PropertyInfo propertyInfo = ReflectionUtil.GetInstanceOrStaticPropertyInfo(lastType, fieldName);
                            lastType = propertyInfo.PropertyType;
                            if (ReflectionUtil.IsPropertyStatic(propertyInfo)) {
                                parts[i] = new AccessExpressionPart_StaticProperty(propertyInfo);
                            }
                            else {
                                parts[i] = new AccessExpressionPart_Property(fieldName);
                            }
                        }

                        continue;
                    }

                    ArrayAccessExpressionNode arrayPart = part as ArrayAccessExpressionNode;
                    if (arrayPart != null) {
                        Expression<int> indexExpression = (Expression<int>) Visit(arrayPart.expressionNode);
                        lastType = ReflectionUtil.GetArrayElementTypeOrThrow(lastType);
                        parts[i] = new AccessExpressionPart_List(indexExpression);
                        continue;
                    }

                    throw new Exception("Unknown AccessExpression Type: " + part.GetType());
                }
            }

            if (isRootContext) {
                parts[0] = new AccessExpressionPart_Field(contextName);
            }

            switch (expressionType) {
                case AccessExpressionType.AliasLookup:
                case AccessExpressionType.RootLookup:
                    ReflectionUtil.ObjectArray2[0] = arg0;
                    ReflectionUtil.ObjectArray2[1] = parts;
                    ReflectionUtil.TypeArray2[0] = lastType;
                    ReflectionUtil.TypeArray2[1] = isRootContext ? context.rootType : headType;
                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpression<,>),
                        ReflectionUtil.TypeArray2,
                        ReflectionUtil.ObjectArray2
                    );

                case AccessExpressionType.StaticField:
                    ReflectionUtil.ObjectArray2[0] = arg0;
                    ReflectionUtil.ObjectArray2[1] = parts;
                    ReflectionUtil.TypeArray1[0] = lastType;
                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpressionStaticField<>),
                        ReflectionUtil.TypeArray1,
                        ReflectionUtil.ObjectArray2
                    );

                case AccessExpressionType.StaticProperty:
                    ReflectionUtil.ObjectArray2[0] = arg0;
                    ReflectionUtil.ObjectArray2[1] = parts;
                    ReflectionUtil.TypeArray1[0] = lastType;
                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpressionStaticProperty<>),
                        ReflectionUtil.TypeArray1,
                        ReflectionUtil.ObjectArray2
                    );
                case AccessExpressionType.Constant:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Expression VisitConstant(LiteralValueNode node) {
            if (node is NumericLiteralNode) {
                return VisitNumericLiteralNode((NumericLiteralNode) node);
            }

            if (node is BooleanLiteralNode) {
                return new ConstantExpression<bool>(((BooleanLiteralNode) node).value);
            }

            if (node is StringLiteralNode) {
                return new ConstantExpression<string>(((StringLiteralNode) node).value);
            }

            return null;
        }

        private static Expression VisitNumericLiteralNode(NumericLiteralNode node) {
            if (node is FloatLiteralNode) {
                return new ConstantExpression<float>(((FloatLiteralNode) node).value);
            }

            if (node is IntLiteralNode) {
                return new ConstantExpression<int>(((IntLiteralNode) node).value);
            }

            return new ConstantExpression<double>(((DoubleLiteralNode) node).value);
        }

        private Expression VisitOperator_TernaryCondition(OperatorExpressionNode node) {
            Expression<bool> condition = (Expression<bool>) Visit(node.left);
            OperatorExpressionNode select = (OperatorExpressionNode) node.right;

            if (select.OpType != OperatorType.TernarySelection) {
                throw new Exception("Bad ternary");
            }

            Expression right = Visit(select.right);
            Expression left = Visit(select.left);

            // todo -- need to assert a type match here
            Type commonBase = ReflectionUtil.GetCommonBaseClass(right.YieldedType, left.YieldedType);

            if (commonBase == null || commonBase == typeof(ValueType) || commonBase == typeof(object)) {
                throw new Exception(
                    "Types in ternary don't match: {right.YieldedType.Name} is not {left.YieldedType.Name}");
            }

            if (commonBase == typeof(int)) {
                return new OperatorExpression_Ternary<int>(
                    condition,
                    (Expression<int>) left,
                    (Expression<int>) right
                );
            }

            if (commonBase == typeof(float)) {
                return new OperatorExpression_Ternary<float>(
                    condition,
                    (Expression<float>) left,
                    (Expression<float>) right
                );
            }

            if (commonBase == typeof(double)) {
                return new OperatorExpression_Ternary<double>(
                    condition,
                    (Expression<double>) left,
                    (Expression<double>) right
                );
            }

            if (commonBase == typeof(string)) {
                return new OperatorExpression_Ternary<string>(
                    condition,
                    (Expression<string>) left,
                    (Expression<string>) right
                );
            }

            if (commonBase == typeof(bool)) {
                return new OperatorExpression_Ternary<bool>(
                    condition,
                    (Expression<bool>) left,
                    (Expression<bool>) right
                );
            }

            Type openType = typeof(OperatorExpression_Ternary<>);
            ReflectionUtil.ObjectArray3[0] = condition;
            ReflectionUtil.ObjectArray3[1] = left;
            ReflectionUtil.ObjectArray3[2] = right;
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(openType, commonBase, ReflectionUtil.ObjectArray3);
        }

    }

}
//
//private Expression VisitMethodCallExpression_Static(MethodInfo info, MethodCallNode node) {
//            string methodName = node.identifierNode.identifier;
//
//            IReadOnlyList<ExpressionNode> signatureParts = node.signatureNode.parts;
//
//            ParameterInfo[] parameters = info.GetParameters();
//
//            if (parameters.Length != signatureParts.Count) {
//                throw new Exception("Argument count is wrong");
//            }
//
//            Expression[] args = new Expression[signatureParts.Count];
//            Type[] genericArguments = new Type[signatureParts.Count + 1];
//
//            for (int i = 0; i < args.Length; i++) {
//                Type requiredType = parameters[i].ParameterType;
//                ExpressionNode argumentNode = signatureParts[i];
//                Expression argumentExpression = Visit(argumentNode);
//                args[i] = HandleCasting(requiredType, argumentExpression);
//
//                genericArguments[i] = args[i].YieldedType;
//            }
//
//            genericArguments[genericArguments.Length - 1] = info.ReturnType;
//
//            ValidateParameterTypes(parameters, args);
//
//            Type callType;
//            switch (args.Length) {
//                case 0:
//                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<>), genericArguments);
//                    break;
//
//                case 1:
//                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,>), genericArguments);
//                    break;
//
//                case 2:
//                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,>), genericArguments);
//                    break;
//
//                case 3:
//                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,>), genericArguments);
//                    break;
//
//                case 4:
//                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,,>), genericArguments);
//                    break;
//
//                default:
//                    throw new Exception($"Expressions only support functions with up to 4 arguments. {methodName} is supplying {args.Length} ");
//            }
//
//            ReflectionUtil.ObjectArray2[0] = info;
//            ReflectionUtil.ObjectArray2[1] = args;
//
//            return (Expression) ReflectionUtil.CreateGenericInstance(callType, ReflectionUtil.ObjectArray2);
//        }