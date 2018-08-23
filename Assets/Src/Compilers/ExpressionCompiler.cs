using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Src.Compilers.CastHandlers;

namespace Src {

    public class ExpressionCompiler {

        private readonly ContextDefinition context;
        private List<ICastHandler> userCastHandlers;

        private static readonly ExpressionParser parser = new ExpressionParser();

        private static readonly List<ICastHandler> builtInCastHandlers = new List<ICastHandler>() {
            new CastHandler_ColorToVector4(),
            new CastHandler_DoubleToMeasurement(),
            new CastHandler_FloatToInt(),
            new CastHandler_FloatToMeasurement(),
            new CastHandler_IntToDouble(),
            new CastHandler_IntToFloat(),
            new CastHandler_IntToMeasurement(),
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
            return Visit(parser.Parse(source));
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

        private void ValidateParameterTypes(ParameterInfo[] parameters, Expression[] arguments) {
            for (int i = 0; i < parameters.Length; i++) {
                if (!parameters[i].ParameterType.IsAssignableFrom(arguments[i].YieldedType)) {
                    throw new Exception($"Cannot use parameter of type {arguments[i].YieldedType} for parameter of type {parameters[i].ParameterType}");
                }
            }
        }

        private Expression VisitMethodCallExpression_Static(MethodInfo info, MethodCallNode node) {
            string methodName = node.identifierNode.identifier;

            IReadOnlyList<ExpressionNode> signatureParts = node.signatureNode.parts;

            ParameterInfo[] parameters = info.GetParameters();

            if (parameters.Length != signatureParts.Count) {
                throw new Exception("Argument count is wrong");
            }

            Expression[] args = new Expression[signatureParts.Count];
            Type[] genericArguments = new Type[signatureParts.Count + 1];

            for (int i = 0; i < args.Length; i++) {
                Type requiredType = parameters[i].ParameterType;
                ExpressionNode argumentNode = signatureParts[i];
                Expression argumentExpression = Visit(argumentNode);
                args[i] = HandleCasting(requiredType, argumentExpression);

                genericArguments[i] = args[i].YieldedType;
            }

            genericArguments[genericArguments.Length - 1] = info.ReturnType;

            ValidateParameterTypes(parameters, args);

            Type callType;
            switch (args.Length) {
                case 0:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<>), genericArguments);
                    break;

                case 1:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,>), genericArguments);
                    break;

                case 2:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,>), genericArguments);
                    break;

                case 3:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,>), genericArguments);
                    break;

                case 4:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Static<,,,,>), genericArguments);
                    break;

                default:
                    throw new Exception($"Expressions only support functions with up to 4 arguments. {methodName} is supplying {args.Length} ");
            }

            ReflectionUtil.ObjectArray2[0] = info;
            ReflectionUtil.ObjectArray2[1] = args;

            return (Expression) ReflectionUtil.CreateGenericInstance(callType, ReflectionUtil.ObjectArray2);
        }

        private Expression VisitMethodCallExpression(MethodCallNode node) {
            string methodName = node.identifierNode.identifier;

            Type[] yieldedTypes = node.signatureNode.parts.Select(p => p.GetYieldedType(context)).ToArray();
            MethodInfo info = (MethodInfo) context.ResolveConstAlias(methodName, yieldedTypes);

            info = info ?? ReflectionUtil.GetMethodInfo(context.rootType, methodName);
            
            if (info == null) {
                
                throw new Exception($"Cannot find method {methodName} on type {context.rootType.Name} or any registered aliases");
            }

            if (info.IsStatic) {
                return VisitMethodCallExpression_Static(info, node);
            }

            ParameterInfo[] parameters = info.GetParameters();
            IReadOnlyList<ExpressionNode> signatureParts = node.signatureNode.parts;

            if (parameters.Length != signatureParts.Count) {
                throw new Exception("Argument count is wrong");
            }

            Expression[] args = new Expression[signatureParts.Count];
            Type[] genericArguments = new Type[signatureParts.Count + 2];

            genericArguments[0] = context.rootType; // todo -- this means root functions only, no chaining right now! 

            for (int i = 0; i < args.Length; i++) {
                Type requiredType = parameters[i].ParameterType;
                ExpressionNode argumentNode = signatureParts[i];
                Expression argumentExpression = Visit(argumentNode);
                args[i] = HandleCasting(requiredType, argumentExpression);

                genericArguments[i + 1] = args[i].YieldedType;
            }

            genericArguments[genericArguments.Length - 1] = info.ReturnType;

            ValidateParameterTypes(parameters, args);

            Type callType;
            switch (args.Length) {
                case 0:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,>), genericArguments);
                    break;

                case 1:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,>), genericArguments);
                    break;

                case 2:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,>), genericArguments);
                    break;

                case 3:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,,>), genericArguments);
                    break;

                case 4:
                    callType = ReflectionUtil.CreateGenericType(typeof(MethodCallExpression_Instance<,,,,,>), genericArguments);
                    break;

                default:
                    throw new Exception(
                        $"Expressions only support functions with up to 4 arguments. {methodName} is supplying {args.Length} ");
            }

            ReflectionUtil.ObjectArray2[0] = info;
            ReflectionUtil.ObjectArray2[1] = args;

            return (Expression) ReflectionUtil.CreateGenericInstance(callType, ReflectionUtil.ObjectArray2);
        }

        private Expression VisitAliasNode(AliasExpressionNode node) {
            Type aliasedType = context.ResolveRuntimeAliasType(node.alias);

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

            Type propertyType = ReflectionUtil.GetFieldType(context.rootType, fieldName);
            ReflectionUtil.ObjectArray2[0] = context.rootType;
            ReflectionUtil.ObjectArray2[1] = fieldName;
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(AccessExpression_Root<>),
                propertyType,
                ReflectionUtil.ObjectArray2
            );
        }

        private Expression VisitParenNode(ParenExpressionNode node) {
            return ParenExpressionFactory.CreateParenExpression(Visit(node.expressionNode));
        }

        private Expression VisitOperatorExpression(OperatorExpressionNode node) {
            Type leftType = node.left.GetYieldedType(context);
            Type rightType = node.right.GetYieldedType(context);

            switch (node.OpType) {
                case OperatorType.Plus:

                    if (leftType == typeof(string) || rightType == typeof(string)) {
                        Type openType = typeof(OperatorExpression_StringConcat<,>);
                        ReflectionUtil.TypeArray2[0] = leftType;
                        ReflectionUtil.TypeArray2[1] = rightType;
                        ReflectionUtil.ObjectArray2[0] = Visit(node.left);
                        ReflectionUtil.ObjectArray2[1] = Visit(node.right);

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

                    ReflectionUtil.ObjectArray3[0] = node.OpType;
                    ReflectionUtil.ObjectArray3[1] = Visit(node.left);
                    ReflectionUtil.ObjectArray3[2] = Visit(node.right);

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

        private Expression VisitAccessExpression(AccessExpressionNode node) {
            string contextName = node.identifierNode.identifier;
            Type headType = context.ResolveRuntimeAliasType(contextName);

            if (headType == null) {
                throw new Exception("Missing field or alias for access on context: " + contextName);
            }

            if (headType.IsPrimitive) {
                throw new Exception(
                    $"Attempting property access on type {headType.Name} on a primitive field {contextName}");
            }

            int startOffset = 0;
            int partCount = node.parts.Count;

            bool isRootContext = !(node.identifierNode is SpecialIdentifierNode);

            if (isRootContext) {
                startOffset++;
                partCount++;
            }

            Type lastType = headType;
            AccessExpressionPart[] parts = new AccessExpressionPart[partCount];

            for (int i = startOffset; i < partCount; i++) {
                AccessExpressionPartNode part = node.parts[i - startOffset];

                PropertyAccessExpressionPartNode propertyPart = part as PropertyAccessExpressionPartNode;

                if (propertyPart != null) {
                    string fieldName = propertyPart.fieldName;
                    lastType = ReflectionUtil.GetFieldInfoOrThrow(lastType, fieldName).FieldType;
                    parts[i] = new AccessExpressionPart_Field(fieldName);
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

            if (isRootContext) {
                parts[0] = new AccessExpressionPart_Field(contextName);
            }

            AccessExpression retn = new AccessExpression(contextName, lastType, parts);
            return retn;
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