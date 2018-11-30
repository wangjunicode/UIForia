using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UIForia.Compilers.CastHandlers;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {
    
    public struct CompilerContext {

        public readonly Type rootType;
        public readonly Type targetType;
        public readonly Type currentType;
        public readonly string fileName;
        public readonly int lineNumber;
        public readonly int columnNumber;

        public CompilerContext(Type rootType, Type targetType, Type currentType) {
            this.rootType = rootType;
            this.targetType = targetType;
            this.currentType = currentType;
            this.fileName = string.Empty;
            this.lineNumber = -1;
            this.columnNumber = -1;
        }

    }

    public class ExpressionCompiler2 {

        private Type rootType;
        private Type targetType;
        private Type currentType;

        private readonly bool allowLinq;
        private readonly Func<ASTNode, Expression> visit;
        private readonly LightList<ExpressionAliasResolver> aliasResolvers;

        public ExpressionCompiler2(bool allowLinq = false) {
            this.allowLinq = allowLinq;
            this.aliasResolvers = new LightList<ExpressionAliasResolver>();
            this.visit = Visit;
        }

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

        public void AddAliasResolver(ExpressionAliasResolver resolver) {
            for (int i = 0; i < aliasResolvers.Count; i++) {
                if (aliasResolvers[i].aliasName == resolver.aliasName) {
                    throw new ParseException($"Trying to add resolver with alias name {resolver.aliasName} but that alias is already registered");
                }
            }

            aliasResolvers.Add(resolver);
        }

        public void RemoveAliasResolver(ExpressionAliasResolver resolver) {
            aliasResolvers.Remove(resolver);
        }

        public Expression<T> Compile<T>(Type rootType, Type currentType, string input) {
            this.targetType = typeof(T);
            this.rootType = rootType;
            this.currentType = currentType;

            ASTNode astRoot = Parser2.Parse(input);
            return (Expression<T>) Visit(astRoot);
        }

        public Expression<T> Compile<T>(Type rootType, string input) {
            return Compile<T>(rootType, rootType, input);
        }

        private Expression Visit(ASTNode node) {
            switch (node.type) {
                case ASTNodeType.NullLiteral:
                    return VisitNull((LiteralNode) node);

                case ASTNodeType.BooleanLiteral:
                    return VisitBoolean((LiteralNode) node);

                case ASTNodeType.NumericLiteral:
                    return VisitNumeric((LiteralNode) node);

                case ASTNodeType.DefaultLiteral:
                    return VisitDefault((LiteralNode) node);

                case ASTNodeType.StringLiteral:
                    return VisitString((LiteralNode) node);

                case ASTNodeType.Operator:
                    return VisitOperator((OperatorNode) node);

                case ASTNodeType.TypeOf:
                    return VisitTypeOf((TypeNode) node);

                case ASTNodeType.Identifier:
                    return VisitSimpleRootAccess((IdentifierNode) node);

                case ASTNodeType.MethodCall:
                    break;

                case ASTNodeType.UnaryNot:
                    return VisitUnaryNot((UnaryExpressionNode) node);

                case ASTNodeType.UnaryMinus:
                    return VisitUnaryMinus((UnaryExpressionNode) node);

                case ASTNodeType.DirectCast:
                    break;

                case ASTNodeType.ListInitializer:
                    break;

                case ASTNodeType.New:
                    break;

                case ASTNodeType.Paren:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private Expression VisitUnaryMinus(UnaryExpressionNode node) {
            Type oldTargetType = targetType;
            targetType = null;
            Expression expr = Visit(node.expression);
            targetType = oldTargetType;

            Type yieldedType = expr.YieldedType;
            if (IsNumericType(yieldedType)) {
                return UnaryExpression_MinusFactory.Create(expr);
            }

            MethodInfo info = ReflectionUtil.GetUnaryOperator("op_UnaryNegation", yieldedType);
            if (info != null) {
                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(OperatorOverloadExpression<,>),
                    new GenericArguments(yieldedType, info.ReturnType),
                    new ConstructorArguments(expr, info)
                );
            }

            throw new CompileException($"Invalid expression type for Unary Minus: {yieldedType}");
        }
        
        private Expression VisitUnaryNot(UnaryExpressionNode node) {
            Type oldTargetType = targetType;
            targetType = null;
            Expression expr = Visit(node.expression);
            targetType = oldTargetType;

            Type yieldedType = expr.YieldedType;

            if (yieldedType == typeof(bool)) {
                return new UnaryExpression_Boolean((Expression<bool>) expr);
            }

            if (yieldedType == typeof(string)) {
                return new UnaryExpression_StringBoolean((Expression<string>) expr);
            }

            MethodInfo info = ReflectionUtil.GetUnaryOperator(GetOpMethodName(OperatorType.Not), yieldedType);
            if (info != null) {
                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(OperatorOverloadExpression<,>),
                    new GenericArguments(yieldedType, info.ReturnType),
                    new ConstructorArguments(expr, info)
                );
            }

            if (yieldedType.IsClass) {
                return new UnaryExpression_ObjectBoolean((Expression<object>) expr);
            }

            throw new CompileException($"Invalid expression type for Unary Not: {yieldedType}");
        }

        private static bool IsNumericType(Type type) {
            return type == typeof(int)
                   || type == typeof(float)
                   || type == typeof(double);
        }

        private Expression VisitOperator(OperatorNode node) {
            Type oldTarget = targetType;
            targetType = null;

            Expression left = Visit(node.left);
            Expression right = Visit(node.right);
            Type leftType = left.YieldedType;
            Type rightType = right.YieldedType;

            targetType = oldTarget;

            Expression retn = null;

            switch (node.operatorType) {
                case OperatorType.Plus:
                case OperatorType.Minus:
                case OperatorType.Mod:
                case OperatorType.Times:
                case OperatorType.Divide: {
                    if (IsNumericType(left.YieldedType) && IsNumericType(right.YieldedType)) {
                        if (ReflectionUtil.AreNumericTypesCompatible(leftType, rightType)) {
                            retn = OperatorExpression_Arithmetic.Create(node.operatorType, left, right);
                            break;
                        }
                    }

                    MethodInfo info = ReflectionUtil.GetBinaryOperator(GetOpMethodName(node.operatorType), left.YieldedType, right.YieldedType);

                    if (info != null) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(OperatorOverloadExpression<,,>),
                            new GenericArguments(leftType, rightType, leftType),
                            new ConstructorArguments(left, right, info)
                        );
                        break;
                    }

                    if (node.operatorType == OperatorType.Plus && (leftType == typeof(string) || rightType == typeof(string))) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(OperatorExpression_StringConcat<,>),
                            new GenericArguments(leftType, rightType),
                            new ConstructorArguments(left, right)
                        );
                        break;
                    }

                    throw new CompileException($"Invalid expression types ({leftType}, {rightType}) for arithmetic operator {node.operatorType}");
                }
                case OperatorType.TernaryCondition:
                    break;
                case OperatorType.TernarySelection:
                    break;
                case OperatorType.Equals:
                case OperatorType.NotEquals: {
                    MethodInfo info = ReflectionUtil.GetComparisonOperator(GetOpMethodName(node.operatorType), left.YieldedType, right.YieldedType);

                    if (info != null) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(OperatorOverloadExpression<,,>),
                            new GenericArguments(leftType, rightType, typeof(bool)),
                            new ConstructorArguments(left, right, info)
                        );
                        break;
                    }

                    if (IsNumericType(left.YieldedType) && IsNumericType(right.YieldedType)) {
                        if (ReflectionUtil.AreNumericTypesCompatible(leftType, rightType)) {
                            retn = OperatorExpression_Comparison.Create(node.operatorType, left, right);
                            break;
                        }
                    }

                    // only use the crappy version if we actually end up needing to
                    // this version sucks because boxing is involved for structs

                    retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(OperatorExpression_Equality<,>),
                        new GenericArguments(leftType, rightType),
                        new ConstructorArguments(node.operatorType, left, right)
                    );

                    break;
                }
                case OperatorType.GreaterThan:
                case OperatorType.GreaterThanEqualTo:
                case OperatorType.LessThan:
                case OperatorType.LessThanEqualTo: {
                    if (IsNumericType(left.YieldedType) && IsNumericType(right.YieldedType)) {
                        if (ReflectionUtil.AreNumericTypesCompatible(leftType, rightType)) {
                            retn = OperatorExpression_Comparison.Create(node.operatorType, left, right);
                            break;
                        }
                    }

                    MethodInfo info = ReflectionUtil.GetComparisonOperator(GetOpMethodName(node.operatorType), left.YieldedType, right.YieldedType);

                    if (info != null) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(OperatorOverloadExpression<,,>),
                            new GenericArguments(leftType, rightType, typeof(bool)),
                            new ConstructorArguments(left, right, info)
                        );
                        break;
                    }

                    throw new CompileException($"Invalid expression types ({leftType}, {rightType}) for comparison operator {node.operatorType}");
                }
                case OperatorType.And:
                case OperatorType.Or: {
                    if (leftType == typeof(bool) && rightType == typeof(bool)) {
                        retn = new OperatorExpression_AndOrBool(node.operatorType, (Expression<bool>) left, (Expression<bool>) right);
                        break;
                    }

                    MethodInfo info = ReflectionUtil.GetComparisonOperator(GetOpMethodName(node.operatorType), left.YieldedType, right.YieldedType);

                    if (info != null) {
                        // todo maybe dont force the return type to be leftType, allow other types
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(OperatorOverloadExpression<,,>),
                            new GenericArguments(leftType, rightType, leftType),
                            new ConstructorArguments(left, right, info)
                        );
                        break;
                    }

                    if (leftType.IsClass && rightType.IsClass) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(OperatorExpression_AndOrObject<,>),
                            new GenericArguments(leftType, rightType),
                            new ConstructorArguments(node.operatorType, left, right)
                        );
                        break;
                    }

                    if (leftType.IsClass && rightType == typeof(bool)) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(OperatorExpression_AndOrObjectBool<>),
                            new GenericArguments(leftType),
                            new ConstructorArguments(node.operatorType, left, right)
                        );
                        break;
                    }

                    if (leftType == typeof(bool) && rightType.IsClass) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(OperatorExpression_AndOrBoolObject<>),
                            new GenericArguments(rightType),
                            new ConstructorArguments(node.operatorType, left, right)
                        );
                        break;
                    }

                    throw new CompileException($"Invalid expression types ({leftType}, {rightType}) for comparison operator {node.operatorType}");
                }
                case OperatorType.As:
                    break;
                case OperatorType.Is:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (retn != null && targetType != null && !targetType.IsAssignableFrom(retn.YieldedType)) {
                Expression cast = GetImplicitCast(retn, targetType);
                if (cast != null) {
                    return cast;
                }

                //return HandleCasting(retn, targetType);
            }

            return retn;
        }

        private string GetOpMethodName(OperatorType operatorType) {
            switch (operatorType) {
                case OperatorType.Plus:
                    return "op_Addition";
                case OperatorType.Minus:
                    return "op_Subtraction";
                case OperatorType.Divide:
                    return "op_Division";
                case OperatorType.Times:
                    return "op_Multiply";
                case OperatorType.Mod:
                    return "op_Modulus";
                case OperatorType.Equals:
                    return "op_Equality";
                case OperatorType.NotEquals:
                    return "op_Inequality";
                case OperatorType.GreaterThan:
                    return "op_GreaterThan";
                case OperatorType.GreaterThanEqualTo:
                    return "op_GreaterThanOrEqual";
                case OperatorType.LessThan:
                    return "op_LessThan";
                case OperatorType.LessThanEqualTo:
                    return "op_LessThanOrEqual";
                case OperatorType.And:
                    return "op_LogicalAnd";
                case OperatorType.Or:
                    return "op_LogicalOr";
                case OperatorType.Not:
                    return "op_LogicalNot";
                default:
                    return string.Empty;
            }
        }

        private Expression VisitNull(LiteralNode node) {
            if (targetType != null && targetType.IsClass) {
                return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(ConstantExpression<>),
                    new GenericArguments(targetType),
                    new ConstructorArguments(null)
                );
            }
            else if (targetType == null) {
                return new ConstantExpression<object>(null);
            }

            throw new ParseException($"Unable to assign null value to {targetType} because {targetType} is not a reference type");
        }

        private Expression VisitDefault(LiteralNode node) {
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(typeof(ConstantExpression<>),
                new GenericArguments(targetType),
                new ConstructorArguments(Activator.CreateInstance(targetType))
            );
        }

        private Expression VisitBoolean(LiteralNode node) {
            bool value = bool.Parse(node.rawValue);
            if (targetType == null || targetType == typeof(bool)) {
                return new ConstantExpression<bool>(value);
            }

            Expression retn = GetImplicitCastConstant(value, targetType);
            return retn ?? HandleCasting(new ConstantExpression<bool>(value), targetType);
        }

        private Expression VisitString(LiteralNode node) {
            if (targetType == null || targetType == typeof(string)) {
                return new ConstantExpression<string>(node.rawValue);
            }

            Expression retn = GetImplicitCastConstant(node.rawValue, targetType);

            return retn ?? HandleCasting(new ConstantExpression<string>(node.rawValue), targetType);
        }

        private Expression VisitNumeric(LiteralNode node) {
            if (targetType == null) {
                if (node.rawValue.IndexOf('f') != -1) {
                    return new ConstantExpression<float>(float.Parse(node.rawValue.Replace("f", "")));
                }

                if (node.rawValue.IndexOf('.') != -1) {
                    return new ConstantExpression<double>(double.Parse(node.rawValue));
                }

                return new ConstantExpression<int>(int.Parse(node.rawValue));
            }

            if (targetType == typeof(float)) {
                if (float.TryParse(node.rawValue.Replace("f", ""), out float f)) {
                    return new ConstantExpression<float>(f);
                }

                throw new ParseException($"Unable to parse {node.rawValue} as a float value");
            }

            if (targetType == typeof(int)) {
                if (int.TryParse(node.rawValue, out int f)) {
                    return new ConstantExpression<int>(f);
                }

                throw new ParseException($"Unable to parse {node.rawValue} as an int value");
            }

            if (targetType == typeof(double)) {
                if (double.TryParse(node.rawValue, out double f)) {
                    return new ConstantExpression<double>(f);
                }

                throw new ParseException($"Unable to parse {node.rawValue} as a double value");
            }

            if (int.TryParse(node.rawValue, out int intVal)) {
                return GetImplicitCastConstant(intVal, targetType);
            }

            if (double.TryParse(node.rawValue, out double dVal)) {
                return GetImplicitCastConstant(dVal, targetType);
            }

            string floatString = node.rawValue.Replace("f", "");

            if (float.TryParse(floatString, NumberStyles.Float, CultureInfo.InvariantCulture, out float fVal)) {
                return GetImplicitCastConstant(fVal, targetType);
            }

            throw new ParseException($"Unable to handle node {node} as a numeric literal");
        }

        private Expression VisitTypeOf(TypeNode typeNode) {
            TypePath typePath = typeNode.typePath;

            // todo this method isn't fully working
            string constructedTypePath = typePath.GetConstructedPath();

            Type[] generics = rootType.GetGenericArguments();
            for (int i = 0; i < generics.Length; i++) {
                if (generics[i].Name == constructedTypePath) {
                    return new ConstantExpression<Type>(generics[i]);
                }
            }

            Type t = TypeExtensions.GetTypeFromSimpleName(constructedTypePath);
            if (t != null) {
                return new ConstantExpression<Type>(t);
            }

            // todo -- need to load ups from using path
            // todo -- support generics 
            throw new NotImplementedException();
        }

        private Expression VisitSimpleRootAccess(IdentifierNode node) {
            string fieldName = node.name;
            Expression retn = null;

            if (node.IsAlias) {
                for (int i = 0; i < aliasResolvers.Length; i++) {
                    if (aliasResolvers[i].aliasName == fieldName) {
                        // root type, target type, element type
                        CompilerContext context = new CompilerContext(rootType, targetType, currentType);
                        retn = aliasResolvers[i].CompileAsValueExpression2(context, node, visit);
                        if (retn == null) {
                            throw new ParseException();
                        }

                        break;
                    }
                }
            }
            else {
                if (ReflectionUtil.IsField(rootType, fieldName)) {
                    Type fieldType = ReflectionUtil.GetFieldType(rootType, fieldName);
                    if (allowLinq) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(AccessExpression_RootField_Linq<,>),
                            new GenericArguments(fieldType, rootType),
                            new ConstructorArguments(rootType, fieldName)
                        );
                    }
                    else {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(AccessExpression_RootField<>),
                            new GenericArguments(fieldType),
                            new ConstructorArguments(rootType, fieldName)
                        );
                    }
                }

                if (ReflectionUtil.IsProperty(rootType, fieldName)) {
                    Type propertyType = ReflectionUtil.GetPropertyType(rootType, fieldName);
                    if (allowLinq) {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(AccessExpression_RootProperty_Linq<,>),
                            new GenericArguments(propertyType, rootType),
                            new ConstructorArguments(rootType, fieldName)
                        );
                    }
                    else {
                        retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(AccessExpression_RootProperty<>),
                            propertyType,
                            new ConstructorArguments(rootType, fieldName)
                        );
                    }
                }
            }

            if (retn == null) {
                throw new ParseException($"Type {rootType} has no field or property with the name {fieldName}");
            }

            if (targetType != null && !targetType.IsAssignableFrom(retn.YieldedType)) {
                Expression cast = GetImplicitCast(retn, targetType);
                if (cast != null) {
                    return cast;
                }

                throw new ParseException($"Type {rootType}.{fieldName} is not assignable to {targetType} and no implicit conversion exists");
            }

            return retn;
        }

        private static Expression GetImplicitCastConstant<T>(T value, Type targetType) {
            MethodInfo info = ReflectionUtil.GetImplicitConversion(targetType, typeof(T));
            if (info == null) {
                return null;
            }

            ReflectionUtil.ObjectArray1[0] = value;
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(ConstantExpression<>),
                new GenericArguments(targetType),
                new ConstructorArguments(info.Invoke(null, ReflectionUtil.ObjectArray1))
            );
        }

        private static Expression GetImplicitCast(Expression input, Type targetType) {
            MethodInfo info = ReflectionUtil.GetImplicitConversion(targetType, input.YieldedType);
            if (info == null) {
                return HandleCasting(input, targetType);
            }

            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(ImplicitCastExpression<,>),
                new GenericArguments(targetType, input.YieldedType),
                new ConstructorArguments(input, info)
            );
        }

        private static Expression HandleCasting(Expression input, Type requiredType) {
            Type yieldedType = input.YieldedType;

            if (yieldedType == requiredType) {
                return input;
            }

            for (int i = 0; i < builtInCastHandlers.Count; i++) {
                if (builtInCastHandlers[i].CanHandle(requiredType, yieldedType)) {
                    return builtInCastHandlers[i].Cast(requiredType, input);
                }
            }

            return null;
        }

    }

}