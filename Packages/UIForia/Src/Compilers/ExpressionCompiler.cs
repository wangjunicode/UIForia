using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UIForia.Compilers.CastHandlers;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Exceptions;
using UIForia.Expressions;
using UIForia.Extensions;
using UIForia.Parsing.Expression;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class CompilerContext {

        public readonly Type rootType;
        public readonly Type targetType;
        public readonly Type currentType;
        public readonly string fileName;
        public readonly int lineNumber;
        public readonly int columnNumber;
        public readonly ASTNode firstNode;
        public readonly ExpressionCompiler compiler;

        internal CompilerContext(ExpressionCompiler compiler, Type rootType, Type currentType, Type targetType, ASTNode firstNode) {
            this.rootType = rootType;
            this.currentType = currentType;
            this.targetType = targetType;
            this.fileName = string.Empty;
            this.lineNumber = -1;
            this.columnNumber = -1;
            this.compiler = compiler;
            this.firstNode = firstNode;
        }

        public Expression Visit(Type targetType, ASTNode node) {
            return compiler.Visit(targetType, node);
        }

    }

    public enum AccessInfoType {

        Invalid,
        MethodInvoke,
        FuncInvoke,
        ActionInvoke,
        Field,
        Property,
        Index

    }

    public class ExpressionCompiler {

        private Type rootType;
        private Type targetType;
        private Type currentType;

        private readonly bool allowLinq;
        private readonly LightList<ExpressionAliasResolver> aliasResolvers;
        private readonly LightList<string> namespaces = new LightList<string>();

        public ExpressionCompiler(bool allowLinq = false) {
            this.allowLinq = allowLinq;
            this.aliasResolvers = new LightList<ExpressionAliasResolver>();
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

        public void AddNamespace(string namespaceName) {
            namespaces.Add(namespaceName);
        }

        public void RemoveNamespace(string namespaceName) {
            namespaces.Remove(namespaceName);
        }

        public void AddAliasResolver(ExpressionAliasResolver resolver) {
            for (int i = 0; i < aliasResolvers.Count; i++) {
                if (aliasResolvers[i].aliasName == resolver.aliasName) {
                    throw new ParseException($"Trying to add resolver with alias name {resolver.aliasName} but that alias is already registered");
                }
            }

            aliasResolvers.Add(resolver);
        }

        public void SetAliasResolver(ExpressionAliasResolver resolver) {
            for (int i = 0; i < aliasResolvers.Count; i++) {
                if (aliasResolvers[i].aliasName == resolver.aliasName) {
                    aliasResolvers[i] = resolver;
                }
            }

            aliasResolvers.Add(resolver);
        }

        public void RemoveAliasResolver(ExpressionAliasResolver resolver) {
            aliasResolvers.Remove(resolver);
        }
        
        public void RemoveAliasResolver(string resolverName) {
            for (int i = 0; i < aliasResolvers.Count; i++) {
                if (aliasResolvers[i].aliasName == resolverName) {
                    aliasResolvers.RemoveAt(i);
                    return;
                }
            }
        }

        public Expression<T> Compile<T>(Type rootType, Type currentType, string input) {
            this.targetType = typeof(T);
            this.rootType = rootType;
            this.currentType = currentType;
            ASTNode astRoot = null;
            try {
                astRoot = ExpressionParser.Parse(input);
                Expression expression = Visit(astRoot);
                
                if (targetType != null && !targetType.IsAssignableFrom(expression.YieldedType)) {
                    Expression cast = GetImplicitCast(expression, targetType);
                    if (cast != null) {
                        return (Expression<T>) cast;
                    }

                    throw new ParseException($"Type {rootType}.{expression.YieldedType} is not assignable to {targetType} and no implicit conversion exists");
                }

                return (Expression<T>)expression;
            } catch (ParseException e) {
                e.SetFileName(this.rootType.FullName);
                throw;
            } catch (CompileException e) {
                e.SetFileName(rootType.FullName);
                e.SetExpression(input);
                throw;
            } catch (Exception e) {
                throw new TemplateParseException(rootType.AssemblyQualifiedName, e.Message, e, astRoot);                
            }
        }

        public Expression<T> Compile<T>(Type rootType, string input) {
            return Compile<T>(rootType, rootType, input);
        }

        public WriteTargetExpression<T> CompileWriteTarget<T>(Type rootType, string input) {
            return (WriteTargetExpression<T>) CompileWriteTarget(rootType, typeof(T), input);
        }

        public WriteTargetExpression CompileWriteTarget(Type rootType, Type valueType, string input) {
            this.targetType = valueType;
            this.rootType = rootType;
            ASTNode astRoot = ExpressionParser.Parse(input);
            try {
                return VisitWriteTarget(astRoot);
            }
            catch (CompileException ex) {
                ex.SetExpression(input);
                throw;
            }
        }

        public Expression Compile(Type rootType, string input, Type targetType) {
            this.targetType = targetType;
            this.rootType = rootType;
            this.currentType = rootType;
            ASTNode astRoot = ExpressionParser.Parse(input);
            return Visit(astRoot);
        }

        public Expression Compile(Type rootType, Type currentType, string input, Type targetType) {
            this.targetType = targetType;
            this.rootType = rootType;
            this.currentType = currentType;
            ASTNode astRoot = ExpressionParser.Parse(input);
            return Visit(astRoot);
        }

        internal Expression Visit(Type targetType, ASTNode node) {
            Type oldTargetType = this.targetType;
            this.targetType = targetType;
            Expression retn = Visit(node);
            this.targetType = oldTargetType;
            return retn;
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
                    return VisitSimpleRootAccess((IdentifierNode) node, false);

                case ASTNodeType.UnaryNot:
                    return VisitUnaryNot((UnaryExpressionNode) node);

                case ASTNodeType.UnaryMinus:
                    return VisitUnaryMinus((UnaryExpressionNode) node);

                case ASTNodeType.New:
                case ASTNodeType.DirectCast:
                    throw new NotImplementedException();

                case ASTNodeType.ListInitializer:
                    return VisitListExpression((ListInitializerNode) node);

                case ASTNodeType.AccessExpression:
                    return VisitAccessExpression((MemberAccessExpressionNode) node);

                case ASTNodeType.Paren:
                    return ParenExpressionFactory.CreateParenExpression(Visit(((ParenNode) node).expression));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private WriteTargetExpression VisitWriteTarget(ASTNode node) {
            switch (node.type) {
                case ASTNodeType.Identifier: {
                    IdentifierNode idNode = (IdentifierNode) node;
                    string fieldName = idNode.name;
                    if (ReflectionUtil.IsField(rootType, fieldName, out FieldInfo fieldInfo)) {
                        if (fieldInfo.IsInitOnly) {
                            throw new CompileException("Cannot write to a readonly or const field");
                        }

                        if (fieldInfo.IsStatic) {
                            throw new CompileException("Cannot write to a static field");
                        }

                        return (WriteTargetExpression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(FieldPropertyTargetExpression<,>),
                            new GenericArguments(rootType, fieldInfo.FieldType),
                            new ConstructorArguments(fieldInfo, null)
                        );
                    }
                    else if (ReflectionUtil.IsProperty(rootType, fieldName, out PropertyInfo propertyInfo)) {
                        if (!propertyInfo.CanWrite) {
                            throw new CompileException("Cannot write to a non writable property");
                        }

                        if (propertyInfo.SetMethod.IsStatic || propertyInfo.GetMethod.IsStatic) {
                            throw new CompileException("Cannot write to a static property");
                        }

                        return (WriteTargetExpression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(FieldPropertyTargetExpression<,>),
                            new GenericArguments(rootType, propertyInfo.PropertyType),
                            new ConstructorArguments(null, propertyInfo)
                        );
                    }
                    else if (ReflectionUtil.IsMethod(rootType, fieldName, out MethodInfo info)) {
                      
                    }

                    throw new CompileException("Invalid write target expression");
                }

                case ASTNodeType.AccessExpression: {
                    MemberAccessExpressionNode memberNode = (MemberAccessExpressionNode) node;
                    if (memberNode.identifier[0] == '$') {
                        throw new CompileException("Write Access Expressions don't support aliases");
                    }

                    LightList<AccessInfo> accessInfos = GetAccessInfoList(rootType, memberNode);

                    if (accessInfos == null) {
                        throw new CompileException("Invalid write access express");
                    }

                    AccessExpressionPart retn = MakeAccessPartFromInfo(accessInfos, 0, true);

                    Expression fetcher = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpression<,>),
                        new GenericArguments(retn.YieldedType, rootType),
                        new ConstructorArguments(retn)
                    );

                    if (accessInfos.Last.type == AccessInfoType.Index) {
                        if (accessInfos.Last.arguments.Count != 1) {
                            throw new CompileException("Unable to compile write target expressions with multiple index parameters");
                        }

                        return (WriteTargetExpression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(IndexedWriteTargetExpression<,,>),
                            new GenericArguments(targetType, accessInfos.Last.inputType, accessInfos.Last.arguments[0].YieldedType),
                            new ConstructorArguments(fetcher, accessInfos.Last.arguments[0])
                        );
                    }
                    else if (accessInfos.Last.type == AccessInfoType.Field) {
                        if (accessInfos.Last.inputType.IsValueType) {
                            throw new CompileException("cannot use struct types as a write assignment targets");
                        }

                        FieldInfo fieldInfo = accessInfos.Last.fieldInfo;
                        if (fieldInfo.IsInitOnly) {
                            throw new CompileException("Cannot write to a readonly or const field");
                        }

                        if (fieldInfo.IsStatic) {
                            throw new CompileException("Cannot write to a static field");
                        }

                        return (WriteTargetExpression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(MemberWriteTargetExpression<,>),
                            new GenericArguments(accessInfos.Last.inputType, targetType),
                            new ConstructorArguments(fetcher, fieldInfo, null)
                        );
                    }
                    else if (accessInfos.Last.type == AccessInfoType.Property) {
                        if (accessInfos.Last.inputType.IsValueType) {
                            throw new CompileException("cannot use struct types as a write assignment targets");
                        }

                        PropertyInfo propertyInfo = accessInfos.Last.propertyInfo;

                        if (!propertyInfo.CanWrite) {
                            throw new CompileException("Cannot write to a non writable property");
                        }

                        if (propertyInfo.SetMethod.IsStatic || propertyInfo.GetMethod.IsStatic) {
                            throw new CompileException("Cannot write to a static property");
                        }

                        return (WriteTargetExpression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(MemberWriteTargetExpression<,>),
                            new GenericArguments(accessInfos.Last.inputType, targetType),
                            new ConstructorArguments(fetcher, null, propertyInfo)
                        );
                    }

                    return null;
                }

                default:
                    throw new CompileException("Unable to treat " + node.type + " as a write target expression");
            }
        }

        private Expression VisitListExpression(ListInitializerNode node) {
            // todo if we have a target type, try to cast all nodes to that type's element type
            // otherwise use a base type
            Type targetElementType = null;
            if (targetType != null && typeof(IList).IsAssignableFrom(targetType)) {
                if (targetType.IsArray) {
                    targetElementType = targetType.GetElementType();
                }
                else {
                    targetElementType = targetType.GetGenericArguments()[0];
                }
            }

            LightList<Expression> exprList = LightListPool<Expression>.Get();

            LightList<Type> typeList = LightListPool<Type>.Get();
            for (int i = 0;
                i < node.list.Count;
                i++) {
                Expression expr = Visit(targetElementType, node.list[i]);
                exprList.Add(expr);
                typeList.Add(expr.YieldedType);
            }

            Type commonBase = ReflectionUtil.GetCommonBaseClass(typeList);
            if (commonBase == null || commonBase == typeof(ValueType) || commonBase == typeof(object)) {
                throw new Exception("Types in list literal don't match, common base type was: " + commonBase);
            }

            Type expressionType = ReflectionUtil.CreateGenericType(typeof(Expression<>), commonBase);

            Expression retnExpr = null;
            if (targetType == null || targetType.IsArray) {
                IList retnValue = Array.CreateInstance(expressionType, node.list.Count);
                for (int i = 0; i < retnValue.Count; i++) {
                    retnValue[i] = exprList[i];
                }

                retnExpr = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(ArrayLiteralExpression<>),
                    new GenericArguments(targetElementType),
                    new ConstructorArguments(retnValue)
                );
            }

            LightListPool<Expression>.Release(ref exprList);
            LightListPool<Type>.Release(ref typeList);
            return retnExpr;
        }

        private LightList<AccessInfo> GetAccessInfoList(Type rootType, MemberAccessExpressionNode node, bool injectHead = true) {
            LightList<AccessInfo> accessInfoList = LightListPool<AccessInfo>.Get();

            Type lastType = rootType;
            if (injectHead) {
                node.parts.Insert(0, ASTNode.DotAccessNode(node.identifier));
            }

            for (int i = 0;
                i < node.parts.Count;
                i++) {
                ASTNode part = node.parts[i];
                AccessInfo accessInfo = new AccessInfo();
                switch (part) {
                    case DotAccessNode dotAccessNode: {
                        if (i != node.parts.Count - 1) {
                            ASTNode nextPart = node.parts[i + 1];
                            if (nextPart is InvokeNode invokeNode) {
                                MethodInfo info;
                                List<Expression> arguments = VisitMethodArguments(lastType, dotAccessNode.propertyName, invokeNode.parameters, out info);
                                if (info == null) {
                                    throw CompileExceptions.MethodNotFound(lastType, dotAccessNode.propertyName);
                                }

                                AccessInfo.CreateMethodCall(ref accessInfo, info, arguments);
                                lastType = accessInfo.outputType;
                                accessInfoList.Add(accessInfo);
                                i++;
                                continue;
                            }
                        }

                        if (AccessInfo.CreateField(ref accessInfo, lastType, dotAccessNode) || AccessInfo.CreateProperty(ref accessInfo, lastType, dotAccessNode)) {
                            lastType = accessInfo.outputType;
                            accessInfoList.Add(accessInfo);
                            continue;
                        }

                        if (i == 0) {
                            if (injectHead) {
                                node.parts.RemoveAt(0);
                            }

                            return null;
                        }

                        throw CompileExceptions.FieldOrPropertyNotFound(lastType, dotAccessNode.propertyName);
                    }

                    case InvokeNode invokeNode: {
                        // last type
                        if (ReflectionUtil.IsAction(lastType)) {
                            throw new NotImplementedException();
                        }
                        else {
                            // func
                            Type outputType = null;
                            List<Expression> arguments = VisitFuncArguments(lastType, invokeNode.parameters, out outputType);
                            accessInfo.type = AccessInfoType.FuncInvoke;
                            accessInfo.arguments = arguments;
                            accessInfo.inputType = lastType;
                            accessInfo.outputType = outputType;
                            lastType = outputType;
                            accessInfoList.Add(accessInfo);
                            continue;
                        }
                    }

                    case IndexNode indexNode: {
                        // todo allow index operator overloads here
                        Expression indexExpression = Visit(typeof(int), indexNode.expression);
                        if (AccessInfo.CreateIndexer(ref accessInfo, lastType, indexExpression)) {
                            lastType = accessInfo.outputType;
                            accessInfoList.Add(accessInfo);
                            continue;
                        }

                        throw new CompileException("Invalid index accessor");
                    }
                }
            }

            return accessInfoList;
        }

        private Expression VisitStaticAccessExpression(Type headType, MemberAccessExpressionNode node) {
            LightList<AccessInfo> accessInfos = GetAccessInfoList(headType, node, false);
            AccessExpressionPart retn = MakeAccessPartFromInfo(accessInfos, 0, false);
            LightListPool<AccessInfo>.Release(ref accessInfos);
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(AccessExpression_Static<,>),
                new GenericArguments(retn.YieldedType, headType),
                new ConstructorArguments(retn, headType.IsEnum)
            );
        }

        public Expression CompileRestOfChain(Expression root, CompilerContext context) {
            Type yieldedType = root.YieldedType;
            LightList<AccessInfo> accessInfos = GetAccessInfoList(yieldedType, context.firstNode as MemberAccessExpressionNode, false);
            AccessExpressionPart retn = MakeAccessPartFromInfo(accessInfos, 0, false);

            AccessExpressionPart bridge = (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(AliasAccessBridge<,,>),
                new GenericArguments(retn.YieldedType, rootType, root.YieldedType),
                new ConstructorArguments(root, retn)
            );

            Expression expr = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(AccessExpression<,>),
                new GenericArguments(retn.YieldedType, rootType),
                new ConstructorArguments(bridge)
            );
            if (targetType == null) {
                return expr;
            }
            return GetImplicitCast(expr, targetType);
        }

        private Expression VisitAliasAccessExpression(MemberAccessExpressionNode node) {
            ExpressionAliasResolver resolver = GetResolver(node.identifier);
            if (resolver == null) {
                throw new CompileException("Unable to find a resolver for alias " + node.identifier);
            }

// find the return type to find the head type
// access info list needs to replace first node with 
            CompilerContext context = new CompilerContext(this, rootType, currentType, targetType, node);
            if (node.parts[0] is InvokeNode invokeNode) {
                return resolver.CompileAsMethodExpression(context, invokeNode.parameters);
            }

            if (node.parts[0] is DotAccessNode dotAccessNode) {
                return resolver.CompileAsDotExpression(context, dotAccessNode.propertyName);
            }

            if (node.parts[0] is IndexNode indexNode) {
                return resolver.CompileAsIndexExpression(context, indexNode.expression);
            }

            return null;
        }

        private ExpressionAliasResolver GetResolver(string alias) {
            for (int i = 0; i < aliasResolvers.Count; i++) {
                if (aliasResolvers[i].aliasName == alias) {
                    return aliasResolvers[i];
                }
            }

            return null;
        }

        private List<Expression> VisitFuncArguments(Type funcType, List<ASTNode> arguments, out Type outputType) {
            Type[] funcParameterTypes = funcType.GetGenericArguments();
            if (arguments.Count + 1 != funcParameterTypes.Length) {
                throw new CompileException($"parameter count mismatch when compiling func invocation {funcType}");
            }

            List<Expression> expressionArguments = new List<Expression>();
            for (int i = 0; i < arguments.Count; i++) {
                Expression argument = Visit(funcParameterTypes[i], arguments[i]);
                if (argument != null) {
                    expressionArguments.Add(argument);
                }
                else {
                    throw new CompileException("Func parameter type mismatch");
                }
            }

            outputType = funcParameterTypes[funcParameterTypes.Length - 1];
            return expressionArguments;
        }

        private List<Expression> VisitMethodArguments(Type originType, string methodName, List<ASTNode> arguments, out MethodInfo methodInfo) {
            List<MethodInfo> infos = ReflectionUtil.GetMethodsWithName(originType, methodName);
            List<Expression> expressionArguments = new List<Expression>();
            for (int i = 0; i < infos.Count; i++) {
                ParameterInfo[] parameterInfos = infos[i].GetParameters();
                if (parameterInfos.Length != arguments.Count) {
                    continue;
                }

                expressionArguments.Clear();
                bool valid = true;
                for (int j = 0; j < parameterInfos.Length; j++) {
                    Expression argument = Visit(parameterInfos[j].ParameterType, arguments[j]);
                    if (argument != null) {
                        expressionArguments.Add(argument);
                    }
                    else {
                        valid = false;
                        break;
                    }
                }

                if (valid) {
                    methodInfo = infos[i];
                    return expressionArguments;
                }
            }

            methodInfo = null;
            return null;
        }

        private Expression VisitAccessExpression(MemberAccessExpressionNode node) {
            if (node.identifier[0] == '$') {
                return VisitAliasAccessExpression(node);
            }

            LightList<AccessInfo> accessInfos = GetAccessInfoList(rootType, node);
            if (accessInfos == null) {
                Type staticType = TypeProcessor.ResolveType(rootType, node.identifier, namespaces);
                if (staticType == null) {
                    throw new CompileException("Can't resolve type for " + node.identifier);
                }

                return VisitStaticAccessExpression(staticType, node);
            }

            AccessExpressionPart retn = MakeAccessPartFromInfo(accessInfos, 0, false);
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(AccessExpression<,>),
                    new GenericArguments(retn.YieldedType, rootType),
                    new ConstructorArguments(retn)
            );
        }

        private AccessExpressionPart MakeAccessPartFromInfo(LightList<AccessInfo> infos, int index, bool writeTarget) {
            if (writeTarget && index == infos.Count - 1) {
                return null;
            }
            else if (index == infos.Count) {
                return null;
            }

            Type finalOutputType = writeTarget ? infos[infos.Count - 2].outputType : infos[infos.Count - 1].outputType;
            if (finalOutputType == typeof(void)) {
                finalOutputType = typeof(Terminal);
            }

            AccessExpressionPart next = MakeAccessPartFromInfo(infos, index + 1, writeTarget);
            AccessInfo info = infos[index];
            switch (info.type) {
                case AccessInfoType.Invalid:
                    return null;
                case AccessInfoType.MethodInvoke:
                    Expression expr = MethodExpressionFactory.CreateMethodExpression(info.methodInfo, info.arguments);
                    return (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpressionPart_Method<,,>),
                        new GenericArguments(finalOutputType, info.inputType, info.outputType),
                        new ConstructorArguments(expr, next)
                    );
                case AccessInfoType.FuncInvoke:
                    Type t0 = info.arguments.Count > 0 ? info.arguments[0].YieldedType : typeof(Terminal);
                    Type t1 = info.arguments.Count > 1 ? info.arguments[1].YieldedType : typeof(Terminal);
                    Type t2 = info.arguments.Count > 2 ? info.arguments[2].YieldedType : typeof(Terminal);
                    Type t3 = info.arguments.Count > 3 ? info.arguments[3].YieldedType : typeof(Terminal);
                    return (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpressionPart_Func<,,,,,,>),
                        new[] {finalOutputType, info.inputType, info.outputType, t0, t1, t2, t3},
                        new ConstructorArguments(info.arguments, next)
                    );
                case AccessInfoType.ActionInvoke:
                    throw new NotImplementedException();
                case AccessInfoType.Field:
                    return (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpressionPart_FieldProperty_Field<,,>),
                        new GenericArguments(finalOutputType, info.inputType, info.outputType),
                        new ConstructorArguments(info.fieldInfo, next)
                    );
                case AccessInfoType.Property:
                    return (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpressionPart_FieldProperty_Property<,,>),
                        new GenericArguments(finalOutputType, info.inputType, info.outputType),
                        new ConstructorArguments(info.propertyInfo, next)
                    );
                case AccessInfoType.Index:
                    return (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AccessExpressionPart_Index<,,>),
                        new GenericArguments(finalOutputType, info.inputType, info.outputType),
                        new ConstructorArguments(next, info.arguments[0])
                    );
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AccessExpressionPart MakeAccessPart(int u, List<ASTNode> parts, Type outputType, LightList<Type> inputTypes) {
            AccessExpressionPart retn = null;
            retn = MakeFieldAccessor(u, parts, outputType, inputTypes); // todo -- field could be a func / action 
            retn = retn ?? MakePropertyAccessor(u, parts, outputType, inputTypes);
            retn = retn ?? MakeMethodAccessor(u, parts, outputType, inputTypes);
//            retn = retn ?? MakeFuncActionAccessor(u, parts, outputType, inputTypes);
            retn = retn ?? MakeIndexAccessor(u, parts, outputType, inputTypes);
            return retn;
        }

        private AccessExpressionPart MakeMethodAccessor(int u, List<ASTNode> parts, Type outputType, LightList<Type> inputTypes) {
            if (u >= parts.Count - 1) {
                return null;
            }

            DotAccessNode dotAccessNode = parts[u] as DotAccessNode;
            if (dotAccessNode == null) {
                return null;
            }

// next needs to be an invoke node
            if ((parts[u + 1] is InvokeNode invokeNode)) {
// find method info for type and make method
                MethodInfo info = ReflectionUtil.GetMethodInfo(inputTypes[u], dotAccessNode.propertyName);
            }

            return null;
        }

        private AccessExpressionPart MakeIndexAccessor(int u, List<ASTNode> parts, Type outputType, LightList<Type> inputTypes) {
            if (u == parts.Count || parts[u].type != ASTNodeType.IndexExpression) {
                return null;
            }

            AccessExpressionPart next = MakeAccessPart(u + 1, parts, outputType, inputTypes);
// todo -- this should handle dictionary indexing as well, pick up target type 
            IndexNode indexNode = (IndexNode) parts[u];
            Expression indexExpr = Visit(typeof(int), indexNode.expression);
            return (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(AccessExpressionPart_Index<,,>),
                new GenericArguments(outputType, inputTypes[u], inputTypes[u + 1]),
                new ConstructorArguments(next, indexExpr)
            );
        }

        private AccessExpressionPart MakePropertyAccessor(int u, List<ASTNode> parts, Type outputType, LightList<Type> inputTypes) {
            if (u == parts.Count || parts[u].type != ASTNodeType.DotAccess) {
                return null;
            }

            string propertyName = ((DotAccessNode) parts[u]).propertyName;
            PropertyInfo propertyInfo = ReflectionUtil.GetInstanceOrStaticPropertyInfo(inputTypes[u], propertyName);
            if (propertyInfo != null) {
                AccessExpressionPart next = MakeAccessPart(u + 1, parts, outputType, inputTypes);
                return (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(AccessExpressionPart_FieldProperty_Property<,,>),
                    new GenericArguments(outputType, inputTypes[u], inputTypes[u + 1]),
                    new ConstructorArguments(propertyInfo, next)
                );
            }

            return null;
        }

        private AccessExpressionPart MakeFieldAccessor(int u, List<ASTNode> parts, Type outputType, LightList<Type> inputTypes) {
            if (u == parts.Count || parts[u].type != ASTNodeType.DotAccess) {
                return null;
            }

            FieldInfo fieldInfo = ReflectionUtil.GetInstanceOrStaticFieldInfo(inputTypes[u], ((DotAccessNode) parts[u]).propertyName);
            if (fieldInfo != null) {
                AccessExpressionPart next = MakeAccessPart(u + 1, parts, outputType, inputTypes);
                return (AccessExpressionPart) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(AccessExpressionPart_FieldProperty_Field<,,>),
                    new GenericArguments(outputType, inputTypes[u], inputTypes[u + 1]),
                    new ConstructorArguments(fieldInfo, next)
                );
            }

            return null;
        }

        private static Type GetListElementType(Type type) {
            if (type.IsArray) {
                return type.GetElementType();
            }

            if (!typeof(System.Collections.IList).IsAssignableFrom(type)) {
                PropertyInfo propertyInfo = type.GetProperty("Item");
                if (propertyInfo != null) {
                    return propertyInfo.PropertyType;
                }

                throw new CompileException($"{type} is not indexable but is being used as in indexer");
            }

            return type.GetGenericArguments()[0];
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
            if (node.operatorType == OperatorType.TernaryCondition) {
                return VisitOperator_TernaryCondition(node);
            }

            Expression left = Visit(node.left);
            Type leftType = left.YieldedType;
            Expression right = Visit(node.right);
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
                case OperatorType.Is:
// todo -- implement these when type resolution works
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
                if (float.TryParse(node.rawValue.Replace("f", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) {
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
                return targetType == null ? new ConstantExpression<int>(intVal) : GetImplicitCastConstant(intVal, targetType);
            }

            if (double.TryParse(node.rawValue, out double dVal)) {
                return targetType == null ? new ConstantExpression<double>(dVal) : GetImplicitCastConstant(dVal, targetType);
            }

            string floatString = node.rawValue.Replace("f", "");
            if (float.TryParse(floatString, NumberStyles.Float, CultureInfo.InvariantCulture, out float fVal)) {
                return targetType == null ? new ConstantExpression<float>(fVal) : GetImplicitCastConstant(fVal, targetType);
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

        private Expression VisitOperator_TernaryCondition(OperatorNode node) {
            Expression<bool> condition = (Expression<bool>) Visit(typeof(bool), node.left);
            OperatorNode select = (OperatorNode) node.right;
            if (select.operatorType != OperatorType.TernarySelection) {
                throw new Exception("Bad ternary");
            }

            Expression right = Visit(select.right);
            Expression left = Visit(select.left);

// todo -- need to assert a type match here
            Type commonBase = ReflectionUtil.GetCommonBaseClass(right.YieldedType, left.YieldedType);
            if (commonBase == null || commonBase == typeof(ValueType) || commonBase == typeof(object)) {
                throw new Exception(
                    $"Types in ternary don't match: {right.YieldedType.Name} is not {left.YieldedType.Name}");
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

        private Expression VisitSimpleRootAccess(IdentifierNode node, bool isWriteTarget) {
            string fieldName = node.name;
            Expression retn = null;
            if (node.IsAlias) {
                for (int i = 0; i < aliasResolvers.Count; i++) {
                    if (aliasResolvers[i].aliasName == fieldName) {
// root type, target type, element type
                        CompilerContext context = new CompilerContext(this, rootType, targetType, currentType, node);
                        retn = aliasResolvers[i].CompileAsValueExpression(context);
                        if (retn == null) {
                            throw new CompileException($"Alias Resolver of type {aliasResolvers[i]} failed to resolve {fieldName}");
                        }

                        break;
                    }
                }

                if (retn == null) {
                    throw new ParseException($"Unknown alias {fieldName}");
                }
            }
            else if (isWriteTarget) {
                if (ReflectionUtil.IsField(rootType, fieldName, out FieldInfo fieldInfo)) {
                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(FieldPropertyTargetExpression<,>),
                        new GenericArguments(rootType, fieldInfo.FieldType),
                        new ConstructorArguments(fieldInfo, null)
                    );
                }
                else if (ReflectionUtil.IsProperty(rootType, fieldName, out PropertyInfo propertyInfo)) {
                    return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(FieldPropertyTargetExpression<,>),
                        new GenericArguments(rootType, propertyInfo.PropertyType),
                        new ConstructorArguments(null, propertyInfo)
                    );
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

                FieldInfo constantField = ReflectionUtil.GetConstantField(rootType, fieldName);
                if (constantField != null) {
                    retn = (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(ConstantExpression<>),
                        new GenericArguments(constantField.FieldType),
                        new ConstructorArguments(constantField.GetValue(null))
                    );
                }
                else if (ReflectionUtil.IsMethod(rootType, fieldName, out MethodInfo methodInfo)) {
                    if (ReflectionUtil.IsAction(targetType)) {
                        
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
            if (requiredType == null || yieldedType == requiredType || requiredType.IsAssignableFrom(yieldedType)) {
                return input;
            }

            MethodInfo info = ReflectionUtil.GetImplicitConversion(requiredType, input.YieldedType);
            for (int i = 0; i < builtInCastHandlers.Count; i++) {
                if (builtInCastHandlers[i].CanHandle(requiredType, yieldedType)) {
                    return builtInCastHandlers[i].Cast(requiredType, input);
                }
            }

            return null;
        }

        public void AddNamespaces(IList<string> usings) {
            if (usings == null) return;
            namespaces.AddRange(usings);
        }

        public void RemoveNamespaces(IList<string> usings) {
            if (usings == null) return;
            for (int i = 0; i < usings.Count; i++) {
                namespaces.Remove(usings[i]);
            }
        }

        public struct AccessInfo {

            public FieldInfo fieldInfo;
            public PropertyInfo propertyInfo;
            public MethodInfo methodInfo;
            public List<Expression> arguments;
            public Type outputType;
            public AccessInfoType type;
            public Type inputType;

            public static void CreateMethodCall(ref AccessInfo accessInfo, MethodInfo info, List<Expression> arguments) {
                accessInfo.type = AccessInfoType.MethodInvoke;
                accessInfo.arguments = arguments;
                accessInfo.methodInfo = info;
                accessInfo.outputType = info.ReturnType == typeof(void) ? typeof(Terminal) : info.ReturnType;
                accessInfo.inputType = info.DeclaringType;
            }

            public static bool CreateField(ref AccessInfo accessInfo, Type lastType, DotAccessNode dotAccessNode) {
                FieldInfo fieldInfo = ReflectionUtil.GetInstanceOrStaticFieldInfo(lastType, dotAccessNode.propertyName);
                if (fieldInfo == null) {
                    return false;
                }

                accessInfo.inputType = lastType;
                accessInfo.fieldInfo = fieldInfo;
                accessInfo.outputType = fieldInfo.FieldType;
                accessInfo.type = AccessInfoType.Field;
                return true;
            }

            public static bool CreateProperty(ref AccessInfo accessInfo, Type lastType, DotAccessNode dotAccessNode) {
                PropertyInfo propertyInfo = ReflectionUtil.GetInstanceOrStaticPropertyInfo(lastType, dotAccessNode.propertyName);
                if (propertyInfo == null) {
                    return false;
                }

                accessInfo.inputType = lastType;
                accessInfo.propertyInfo = propertyInfo;
                accessInfo.outputType = propertyInfo.PropertyType;
                accessInfo.type = AccessInfoType.Property;
                return true;
            }

            public static bool CreateIndexer(ref AccessInfo accessInfo, Type lastType, Expression indexExpression) {
                accessInfo.inputType = lastType;
                accessInfo.outputType = GetListElementType(lastType);
                accessInfo.arguments = new List<Expression>();
                accessInfo.arguments.Add(indexExpression);
                accessInfo.type = AccessInfoType.Index;
                return true;
            }

        }

    }

}