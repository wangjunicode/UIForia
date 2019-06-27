using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Exceptions;
using UIForia.Extensions;
using UIForia.Parsing.Expression;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Util;
using UnityEngine;
using Expression = System.Linq.Expressions.Expression;

namespace UIForia.Compilers {

    public class LinqCompiler {

        // todo -- pool blocks 

        private readonly List<ParameterExpression> parameters;
        private readonly LightStack<BlockDefinition> blockStack;
        private readonly LightList<string> namespaces;

        private string defaultIdentifier;
        private Type returnType;

        public LinqCompiler() {
            this.parameters = new List<ParameterExpression>();
            this.blockStack = new LightStack<BlockDefinition>();
            this.namespaces = new LightList<string>();
            blockStack.Push(new BlockDefinition());
        }

        private BlockDefinition currentBlock => blockStack.Peek();

        public void Reset() {
            parameters.Clear();
            blockStack.Clear();
            namespaces.Clear();
            blockStack.Push(new BlockDefinition());
            defaultIdentifier = null;
            returnType = null;
        }

        public void Assign(LHSStatementChain left, RHSStatementChain right) {
            if (left.isSimpleAssignment) {
                currentBlock.AddStatement(
                    Expression.Assign(left.targetExpression, right.OutputExpression)
                );
            }
            else {
                LHSAssignment[] assignments = left.assignments.array;
                // this avoid one unneeded copy since undoctored this would write to a local that is unused
                assignments[left.assignments.size - 1].left = right.OutputExpression;
                for (int i = left.assignments.size - 1; i >= 0; i--) {
                    currentBlock.AddStatement(Expression.Assign(assignments[i].right, assignments[i].left));
                }
            }
        }

        public void Assign(LHSStatementChain left, Expression expression) {
            if (left.isSimpleAssignment) {
                currentBlock.AddStatement(
                    Expression.Assign(left.targetExpression, expression)
                );
            }
            else {
                LHSAssignment[] assignments = left.assignments.array;
                // this avoid one unneeded copy since undoctored this would write to a local that is unused
                assignments[left.assignments.size - 1].left = expression;
                for (int i = left.assignments.size - 1; i >= 0; i--) {
                    currentBlock.AddStatement(Expression.Assign(assignments[i].right, assignments[i].left));
                }

                // for each assignment in left.assignments
                // Assign(assignment.right, assignment.left);
                // if assignment.type == index
                // do index expression

                // float outputValue = expression;
                // value.x = outputValue;
                // svHolderVec3.value = value;
                // element.svHolderVec3 = svHolderVec3;
            }
        }

        public void SetDefaultIdentifier(string identifierName) {
            this.defaultIdentifier = identifierName;
        }

        public void AddNamespace(string namespaceName) {
            if (string.IsNullOrEmpty(namespaceName)) return;
            if (namespaces.Contains(namespaceName)) return;
            namespaces.Add(namespaceName);
        }

        public void SetReturnType(Type returnType) {
            this.returnType = returnType;
        }

        public void Assign(Expression left, Expression right) {
            currentBlock.AddStatement(Expression.Assign(left, right));
        }

        public void ReturnStatement(Expression expression) {
            currentBlock.AddStatement(expression);
        }

        public void Invoke(Expression target, MethodInfo methodInfo, params Expression[] args) { }

        // todo -- support strings and or expressions or constants
        public void Invoke(string targetVariableName, string methodName, params Expression[] args) { }

        public void ForEach(IEnumerable list, Action<ParameterExpression> block) { }

        public void IfEqual(LHSStatementChain left, RHSStatementChain right, Action body) { }

        public void IfEqual(Expression left, Expression right, Action body) {
            Expression condition = Expression.Equal(left, right);

            BlockDefinition bodyBlock = new BlockDefinition();

            blockStack.Push(bodyBlock);

            body();

            blockStack.PopUnchecked();

            currentBlock.AddStatement(Expression.IfThen(condition, bodyBlock.ToExpressionBlock(typeof(void))));
        }

        public void IfNotEqual(LHSStatementChain left, RHSStatementChain right, Action body) {
            Debug.Assert(left != null);
            Debug.Assert(right.OutputExpression != null);
            Debug.Assert(body != null);

            Expression condition = Expression.NotEqual(left.targetExpression, right.OutputExpression);

            BlockDefinition bodyBlock = new BlockDefinition();

            blockStack.Push(bodyBlock);

            body();

            blockStack.PopUnchecked();

            currentBlock.AddStatement(Expression.IfThen(condition, bodyBlock.ToExpressionBlock(typeof(void))));
        }

        public LambdaExpression BuildLambda() {
            Debug.Assert(blockStack.Count == 1);
            // todo -- return type is wrong probably
            return Expression.Lambda(currentBlock.ToExpressionBlock(returnType ?? typeof(void)), parameters.ToArray());
        }

        public Expression<T> BuildLambda<T>() where T : Delegate {
            Debug.Assert(blockStack.Count == 1);
            Expression<T> expr;
            if (ReflectionUtil.IsAction(typeof(T))) {
                Type[] genericArguments = typeof(T).GetGenericArguments();
                if (parameters.Count != genericArguments.Length) {
                    throw CompileException.InvalidActionArgumentCount(parameters, genericArguments);
                }

                expr = Expression.Lambda<T>(currentBlock.ToExpressionBlock(typeof(void)), parameters.ToArray());
            }
            else {
                Type[] genericArguments = typeof(T).GetGenericArguments();
                expr = Expression.Lambda<T>(currentBlock.ToExpressionBlock(genericArguments[genericArguments.Length - 1]), parameters.ToArray());
            }

            return expr;
        }

        public T Compile<T>() where T : Delegate {
            return BuildLambda<T>().Compile();
        }

        public ParameterExpression AddParameter(Type type, string name) {
            // todo validate no name conflicts
            ParameterExpression retn = Expression.Parameter(type, name);
            parameters.Add(retn);
            return retn;
        }

        private ParameterExpression ResolveVariableName(string variableName) {
            for (int i = blockStack.Count - 1; i >= 0; i--) {
                ParameterExpression variable = blockStack.PeekAtUnchecked(i).ResolveVariable(variableName);
                if (variable != null) {
                    return variable;
                }
            }

            for (int i = 0; i < parameters.Count; i++) {
                if (parameters[i].Name == variableName) {
                    return parameters[i];
                }
            }

            return null;
        }

        public LHSStatementChain CreateLHSStatementChain(string rootVariableName, string input) {
            ParameterExpression head = ResolveVariableName(rootVariableName);

            ASTNode astRoot = ExpressionParser.Parse(input);

            LHSStatementChain retn = new LHSStatementChain();

            if (astRoot.type == ASTNodeType.Identifier) {
                IdentifierNode idNode = (IdentifierNode) astRoot;
                if (idNode.IsAlias) {
                    throw new InvalidLeftHandStatementException("alias cannot be used in a LHS expression", input);
                }

                // simple case, just store target property in a variable
                retn.isSimpleAssignment = true;
                retn.targetExpression = MemberAccess(head, idNode.name);
            }
            else if (astRoot.type == ASTNodeType.AccessExpression) {
                MemberAccessExpressionNode memberNode = (MemberAccessExpressionNode) astRoot;

                Expression last = MemberAccess(head, memberNode.identifier);

                Expression variable = currentBlock.AddVariable(last.Type, memberNode.identifier);
                currentBlock.AddStatement(Expression.Assign(variable, last));

                retn.AddAssignment(variable, last);

                for (int i = 0; i < memberNode.parts.Count; i++) {
                    // if any part is a method, fail
                    // if any part is read only fail
                    // if any part is a struct need to write back
                    ASTNode part = memberNode.parts[i];

                    if (part is DotAccessNode dotAccessNode) {
                        last = MemberAccess(variable, dotAccessNode.propertyName);
                        variable = currentBlock.AddVariable(last.Type, dotAccessNode.propertyName);
                        currentBlock.AddStatement(Expression.Assign(variable, last));
                        retn.AddAssignment(variable, last);
                    }
                    else if (part is IndexNode indexNode) {
                        // recurse to get index
                    }
                    else if (part is InvokeNode invokeNode) {
                        throw new InvalidLeftHandStatementException(part.type.ToString(), "Cannot use invoke operator () in the lhs");
                    }
                }

                retn.targetExpression = retn.assignments[retn.assignments.size - 1].left;
            }
            else {
                throw new InvalidLeftHandStatementException(astRoot.type.ToString(), input);
            }

            return retn;
        }

        private Expression MemberAccess(Expression head, string fieldOrPropertyName) {
            MemberInfo memberInfo = ReflectionUtil.GetFieldOrProperty(head.Type, fieldOrPropertyName);
            if (memberInfo == null) {
                throw new CompileException($"Type {head.Type} does not declare an accessible field or property with the name {fieldOrPropertyName}");
            }

            return Expression.MakeMemberAccess(head, memberInfo);
        }


        private Expression VisitAccessExpression(MemberAccessExpressionNode accessNode) {
            List<ASTNode> parts = accessNode.parts;

            // assume not an alias for now
            ParameterExpression head = null;
            Expression last = null;

            if (defaultIdentifier != null) {
                head = ResolveVariableName(defaultIdentifier);
                last = MemberAccess(head, accessNode.identifier);
            }
            else {
                head = ResolveVariableName(accessNode.identifier);
                // todo this needs work, need to start parts access at 1 probably and visit the first immediately or 'last' will be null. maybe we can just set last = head?
                throw new NotImplementedException("Haven't implemented non implicit access expressions yet");
            }

            bool needsNullChecking = false;

            LabelTarget returnTarget = Expression.Label("retn");

            Expression lastValue = currentBlock.AddVariable(last.Type, "part");
            currentBlock.AddStatement(Expression.Assign(lastValue, last));

            // need a variable when we hit a reference type
            // structs do not need intermediate variables, in fact due to the copy cost its best not to have them for structs at all
            // properties are always read into fields, we assume they are more expensive and worth local caching even when structs
            // need to read from local variable if last thing we hit was

            for (int i = 0; i < parts.Count; i++) {
                if (parts[i] is DotAccessNode dotAccessNode) {
                    // if the last thing checked was a class, we need a null check
                    if (last.Type.IsClass) {
                        needsNullChecking = true;

                        // cascade a null check, if we are looking up a value and trying to read from something that is null,
                        // then we jump to the end of value chain and use default(inputType) as a final value
                        currentBlock.AddStatement(NullCheck(lastValue, returnTarget));
                    }

                    last = MemberAccess(lastValue, dotAccessNode.propertyName);

                    // todo -- optimization would be to do this while the next part is dot access & a struct so we don't over copy    

                    lastValue = currentBlock.AddVariable(last.Type, "part" + i);
                    currentBlock.AddAssignment(lastValue, last);
                }
                else if (parts[i] is IndexNode indexNode) {
                    Type lastValueType = lastValue.Type;
                    // todo -- also no support for multiple index properties right now, parser needs to accept a comma list for that to work

                    if (lastValueType.IsArray) {
                        needsNullChecking = true;

                        if (lastValueType.GetArrayRank() != 1) {
                            throw new NotSupportedException("Expressions do not support multidimensional arrays yet");
                        }

                        Expression indexExpression = Visit(typeof(int), indexNode.expression);
                        if (indexExpression.Type != typeof(int)) {
                            indexExpression = Expression.Convert(indexExpression, typeof(int));
                        }

                        Expression indexer = currentBlock.AddVariable(indexExpression.Type, "indexer");
                        currentBlock.AddStatement(Expression.Assign(indexer, indexExpression));
                        currentBlock.AddStatement(NullAndBoundsCheck(indexer, lastValue, "Length", returnTarget));

                        last = Expression.ArrayAccess(lastValue, indexer);
                        lastValue = currentBlock.AddVariable(lastValueType.GetElementType(), "arrayVal");
                        currentBlock.AddStatement(Expression.Assign(lastValue, last));
                    }
                    else {
                        bool isList = lastValueType.Implements(typeof(IList));

                        Expression indexExpression = Visit(isList ? typeof(int) : null, indexNode.expression);
                        indexExpression = FindIndexExpression(lastValueType, indexExpression, out PropertyInfo indexProperty);
                        Expression indexer = currentBlock.AddVariable(indexExpression.Type, "indexer");
                        currentBlock.AddStatement(Expression.Assign(indexer, indexExpression));

                        if (isList) {
                            needsNullChecking = true;
                            currentBlock.AddStatement(NullAndBoundsCheck(indexer, lastValue, "Count", returnTarget));
                        }
                        else if (lastValueType.IsClass) {
                            needsNullChecking = true;
                            currentBlock.AddStatement(NullCheck(lastValue, returnTarget));
                        }

                        last = Expression.MakeIndex(lastValue, indexProperty, new[] {indexer});
                        lastValue = currentBlock.AddVariable(indexProperty.PropertyType, "indexVal");
                        currentBlock.AddStatement(Expression.Assign(lastValue, last));
                    }
                }
            }

            ParameterExpression output = currentBlock.AddVariable(lastValue.Type, "rhsOutput");
            currentBlock.PrependStatement(Expression.Assign(output, Expression.Default(output.Type)));

            currentBlock.AddStatement(Expression.Assign(output, lastValue));

            if (needsNullChecking) {
                currentBlock.AddStatement(Expression.Label(returnTarget));
            }

            return output;
        }

        private Expression NullAndBoundsCheck(Expression indexExpression, Expression variable, string field, LabelTarget returnTarget) {
            if (variable.Type.IsClass) {
                return Expression.IfThen(
                    Expression.OrElse(
                        Expression.Equal(variable, Expression.Constant(null)),
                        Expression.OrElse(
                            Expression.LessThan(indexExpression, Expression.Constant(0)),
                            Expression.GreaterThanOrEqual(indexExpression, MemberAccess(variable, field))
                        )),
                    Expression.Goto(returnTarget)
                );
            }
            else {
                return Expression.IfThen(
                    Expression.OrElse(
                        Expression.LessThan(indexExpression, Expression.Constant(0)),
                        Expression.GreaterThanOrEqual(indexExpression, MemberAccess(variable, field))
                    ),
                    Expression.Goto(returnTarget)
                );
            }
        }

        private static Expression NullCheck(Expression variable, LabelTarget label) {
            return Expression.IfThen(Expression.Equal(variable, Expression.Constant(null)), Expression.Goto(label));
        }

        private static Expression FindIndexExpression(Type type, Expression indexExpression, out PropertyInfo indexProperty) {
            IList<ReflectionUtil.IndexerInfo> indexedProperties = ReflectionUtil.GetIndexedProperties(type, ListPool<ReflectionUtil.IndexerInfo>.Get());
            List<ReflectionUtil.IndexerInfo> l = (List<ReflectionUtil.IndexerInfo>) indexedProperties;

            Type targetType = indexExpression.Type;

            for (int i = 0; i < indexedProperties.Count; i++) {
                if (indexedProperties[i].parameterInfos.Length == 1) {
                    if (indexedProperties[i].parameterInfos[0].ParameterType == targetType) {
                        indexProperty = indexedProperties[i].propertyInfo;
                        ListPool<ReflectionUtil.IndexerInfo>.Release(ref l);
                        return indexExpression;
                    }
                }
            }

            for (int i = 0; i < indexedProperties.Count; i++) {
                // if any conversions exist this will work, if not we hit an exception
                try {
                    indexExpression = Expression.Convert(indexExpression, indexedProperties[i].parameterInfos[0].ParameterType);
                    indexProperty = indexedProperties[i].propertyInfo;
                    ListPool<ReflectionUtil.IndexerInfo>.Release(ref l);

                    return indexExpression;
                }
                catch (Exception) {
                    // ignored
                }
            }

            ListPool<ReflectionUtil.IndexerInfo>.Release(ref l);

            throw new CompileException($"Can't find indexed property that accepts an indexer of type {indexExpression.Type}");
        }

        private Expression Visit(ASTNode node) {
            return Visit(null, node);
        }

        private Expression VisitUnchecked(Type targetType, ASTNode node) {
            switch (node.type) {
                case ASTNodeType.NullLiteral:
                    return Expression.Constant(null);

                case ASTNodeType.BooleanLiteral:
                    return VisitBoolLiteral((LiteralNode) node);

                case ASTNodeType.NumericLiteral:
                    return VisitNumericLiteral(targetType, (LiteralNode) node);

                case ASTNodeType.DefaultLiteral:
                    // todo -- when target type is unknown
                    return null; // Expression.Default(targetType);

                case ASTNodeType.StringLiteral:
                    return Expression.Constant(((LiteralNode) node).rawValue);

                case ASTNodeType.Operator:
                    return VisitOperator((OperatorNode) node);

                case ASTNodeType.TypeOf:
                    return VisitTypeNode((TypeNode) node);

                case ASTNodeType.Identifier:
                    return VisitIdentifierNode((IdentifierNode) node);

                case ASTNodeType.AccessExpression:
                    return VisitAccessExpression((MemberAccessExpressionNode) node);

                case ASTNodeType.UnaryNot:
                    return VisitUnaryNot((UnaryExpressionNode) node);

                case ASTNodeType.UnaryMinus:
                    return VisitUnaryNot((UnaryExpressionNode) node);

                case ASTNodeType.UnaryBitwiseNot:
                    return VisitBitwiseNot((UnaryExpressionNode) node);

                case ASTNodeType.DirectCast:
                    break;

                case ASTNodeType.ListInitializer:
                    // [] if not used as a return value then use pooling for the array 
                    // [1, 2, 3].Contains(myValue)
                    // repeat list="[1, 2, 3]"
                    // style=[style1, style2, property ? style3]
                    // value=new Vector
                    break;

                case ASTNodeType.New:
                    break;

                case ASTNodeType.Paren:
                    ParenNode parenNode = (ParenNode) node;
                    return Visit(parenNode.expression);

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private Expression Visit(Type targetType, ASTNode node) {
            Expression retn = VisitUnchecked(targetType, node);
            if (targetType != null && retn.Type != targetType) {
                try {
                    retn = Expression.Convert(retn, targetType);
                }
                catch (InvalidOperationException ex) {
                    throw CompileException.InvalidTargetType(targetType, retn.Type);
                }
            }

            return retn;
        }

        private Expression CompileIndexExpression(Expression head, IndexNode indexNode) {
            if (head.Type.IsArray) {
                return Visit(typeof(int), indexNode.expression);
            }

            return null;
        }

        private Expression VisitOperator(OperatorNode operatorNode) {
            Expression left;
            Expression right;

            if (operatorNode.operatorType == OperatorType.TernaryCondition) {
                OperatorNode select = (OperatorNode) operatorNode.right;

                if (select.operatorType != OperatorType.TernarySelection) {
                    throw new CompileException("Bad ternary, expected the right hand side to be a TernarySelection but it was {select.operatorType}");
                }

                left = Visit(select.left);
                right = Visit(select.right);

                Expression ternaryCondition = Visit(operatorNode.left);
                Expression conditionVariable = currentBlock.AddVariable(typeof(bool), "ternary");

                currentBlock.AddAssignment(conditionVariable, ternaryCondition);

                // Expression ternaryBody = Expression.IfThenElse(conditionVariable
                throw new NotImplementedException();
            }

            left = Visit(operatorNode.left);
            right = Visit(operatorNode.right);
            try {
                switch (operatorNode.operatorType) {
                    case OperatorType.Plus:
                        return Expression.Add(left, right);

                    case OperatorType.Minus:
                        return Expression.Subtract(left, right);

                    case OperatorType.Mod:
                        return Expression.Modulo(left, right);

                    case OperatorType.Times:
                        return Expression.Multiply(left, right);

                    case OperatorType.Divide:
                        return Expression.Divide(left, right);

                    case OperatorType.Equals:
                        return Expression.Equal(left, right);

                    case OperatorType.NotEquals:
                        return Expression.NotEqual(left, right);

                    case OperatorType.GreaterThan:
                        return Expression.GreaterThan(left, right);

                    case OperatorType.GreaterThanEqualTo:
                        return Expression.GreaterThanOrEqual(left, right);

                    case OperatorType.LessThan:
                        return Expression.LessThan(left, right);

                    case OperatorType.LessThanEqualTo:
                        return Expression.LessThanOrEqual(left, right);

                    case OperatorType.And:
                        return Expression.AndAlso(left, right);

                    case OperatorType.Or:
                        return Expression.OrElse(left, right);

                    case OperatorType.ShiftRight:
                        return Expression.RightShift(left, right);

                    case OperatorType.ShiftLeft:
                        return Expression.LeftShift(left, right);

                    case OperatorType.BinaryAnd:
                        return Expression.And(left, right);

                    case OperatorType.BinaryOr:
                        return Expression.Or(left, right);

                    case OperatorType.BinaryXor:
                        return Expression.ExclusiveOr(left, right);

                    case OperatorType.Is: {
                        TypeNode typeNode = (TypeNode) operatorNode.right;
                        Type t = TypeProcessor.ResolveType(typeNode.typePath.ConstructTypeLookupTree(), namespaces);
                        return Expression.TypeIs(left, t);
                    }

                    case OperatorType.As: {
                        TypeNode typeNode = (TypeNode) operatorNode.right;
                        Type t = TypeProcessor.ResolveType(typeNode.typePath.ConstructTypeLookupTree(), namespaces);
                        return Expression.TypeAs(left, t);
                    }

                    default:
                        throw new CompileException($"Tried to visit the operator node {operatorNode.operatorType} but it wasn't handled by LinqCompiler.VisitOperator");
                }
            }
            catch (InvalidOperationException invalidOp) {
                if (invalidOp.Message.Contains("is not defined for the types")) {
                    throw CompileException.MissingBinaryOperator(operatorNode.operatorType, left.Type, right.Type);
                }
                else throw;
            }
        }

        public RHSStatementChain CreateRHSStatementChain(string input) {
            return CreateRHSStatementChain(null, null, input);
        }

        public RHSStatementChain CreateRHSStatementChain(string defaultIdentifier, Type targetType, string input) {
            ASTNode astRoot = ExpressionParser.Parse(input);

            RHSStatementChain retn = new RHSStatementChain();

            if (defaultIdentifier != null) {
                SetDefaultIdentifier(defaultIdentifier);
            }

            // todo -- not setting default identifier should try to resolve first node by parameter name
            switch (astRoot.type) {
                case ASTNodeType.NullLiteral:
                    retn.OutputExpression = Expression.Constant(null);
                    break;

                case ASTNodeType.BooleanLiteral:
                    retn.OutputExpression = VisitBoolLiteral((LiteralNode) astRoot);
                    break;

                case ASTNodeType.NumericLiteral:
                    retn.OutputExpression = VisitNumericLiteral(targetType, (LiteralNode) astRoot);
                    break;

                case ASTNodeType.DefaultLiteral:
                    retn.OutputExpression = Expression.Default(targetType);
                    break;

                case ASTNodeType.StringLiteral:
                    // todo -- apply escaping here?
                    retn.OutputExpression = Expression.Constant(((LiteralNode) astRoot).rawValue);
                    break;

                case ASTNodeType.Operator:
                    retn.OutputExpression = VisitOperator((OperatorNode) astRoot);
                    break;

                case ASTNodeType.TypeOf:
                    retn.OutputExpression = VisitTypeNode((TypeNode) astRoot);
                    break;

                case ASTNodeType.Identifier: {
                    if (string.IsNullOrEmpty(defaultIdentifier) || string.IsNullOrWhiteSpace(defaultIdentifier)) {
                        throw CompileException.RHSRootIdentifierMissing(((IdentifierNode) astRoot).name);
                    }

                    retn.OutputExpression = VisitIdentifierNode((IdentifierNode) astRoot);
                    break;
                }

                case ASTNodeType.AccessExpression: {
                    retn.OutputExpression = VisitAccessExpression((MemberAccessExpressionNode) astRoot);
                    break;
                }

                case ASTNodeType.IndexExpression:
                    break;

                case ASTNodeType.UnaryNot:
                    retn.OutputExpression = VisitUnaryNot((UnaryExpressionNode) astRoot);
                    break;

                case ASTNodeType.UnaryMinus:
                    retn.OutputExpression = VisitUnaryMinus((UnaryExpressionNode) astRoot);
                    break;

                case ASTNodeType.UnaryBitwiseNot:
                    retn.OutputExpression = VisitBitwiseNot((UnaryExpressionNode) astRoot);
                    break;

                case ASTNodeType.DirectCast:
                    break;

                case ASTNodeType.ListInitializer:
                    break;

                case ASTNodeType.New:
                    break;

                case ASTNodeType.Paren:
                    ParenNode parenNode = (ParenNode) astRoot;
                    retn.OutputExpression = Visit(parenNode.expression);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return retn;
        }

        private Expression VisitUnaryNot(UnaryExpressionNode node) {
            Expression body = Visit(node.expression);
            return Expression.Not(body);
        }

        private Expression VisitUnaryMinus(UnaryExpressionNode node) {
            Expression body = Visit(node.expression);
            return Expression.Negate(body);
        }

        private Expression VisitBitwiseNot(UnaryExpressionNode node) {
            Expression body = Visit(node.expression);
            return Expression.OnesComplement(body);
        }

        private Expression VisitTypeNode(TypeNode typeNode) {
            TypePath typePath = typeNode.typePath;

            try {
                TypeLookup typeLookup = typePath.ConstructTypeLookupTree();
                Type t = TypeProcessor.ResolveType(typeLookup, namespaces);
                return Expression.Constant(t);
            }
            catch (TypeResolutionException) { }

            if (defaultIdentifier != null) {
                // todo -- get rid of typePath and only use TypeLookup
                Expression root = ResolveVariableName(defaultIdentifier);

                if (!root.Type.IsGenericType) {
                    throw new NotImplementedException($"Searching for type {typePath.path[0]} but unable to find it from type {root.Type}. typeof() can currently only resolve generics if {nameof(SetDefaultIdentifier)} has been called");
                }

                Type[] generics = root.Type.GetGenericArguments();
                Type[] baseGenericArguments = root.Type.GetGenericTypeDefinition().GetGenericArguments();

                Debug.Assert(generics.Length == baseGenericArguments.Length);

                for (int i = 0; i < generics.Length; i++) {
                    if (baseGenericArguments[i].Name == typePath.path[0]) {
                        return Expression.Constant(generics[i]);
                    }
                }
            }

            throw CompileException.UnresolvedType(typePath.ConstructTypeLookupTree());
            
        }

        private Expression VisitIdentifierNode(IdentifierNode identifierNode) {
            if (identifierNode.IsAlias) {
                throw new NotImplementedException("Aliases aren't support yet");
            }

            Expression parameterExpression = ResolveVariableName(identifierNode.name);
            if (parameterExpression != null) {
                Expression variable = currentBlock.AddVariable(parameterExpression.Type, identifierNode.name);
                currentBlock.AddStatement(Expression.Assign(variable, parameterExpression));
                return variable;
            }
            else if (defaultIdentifier != null) {
                Expression expr = MemberAccess(ResolveVariableName(defaultIdentifier), identifierNode.name);
                Expression variable = currentBlock.AddVariable(expr.Type, identifierNode.name);
                currentBlock.AddStatement(Expression.Assign(variable, expr));
                return variable;
            }

            throw CompileException.UnresolvedIdentifier(identifierNode.name);
        }

        private static Expression VisitBoolLiteral(LiteralNode literalNode) {
            if (bool.TryParse(literalNode.rawValue, out bool value)) {
                return Expression.Constant(value);
            }

            throw new CompileException($"Unable to parse bool from {literalNode.rawValue}");
        }

        private static Expression VisitNumericLiteral(Type targetType, LiteralNode literalNode) {
            if (targetType == null) {
                string value = literalNode.rawValue.Trim();
                char lastChar = char.ToLower(value[value.Length - 1]);
                if (value.Length > 1) {
                    if (lastChar == 'f') {
                        if (float.TryParse(value.Remove(value.Length - 1), out float fVal)) {
                            return Expression.Constant(fVal);
                        }

                        throw new CompileException($"Tried to parse value {literalNode.rawValue} as a float but failed");
                    }

                    if (lastChar == 'd') {
                        if (double.TryParse(value.Remove(value.Length - 1), out double fVal)) {
                            return Expression.Constant(fVal);
                        }

                        throw new CompileException($"Tried to parse value {literalNode.rawValue} as a double but failed");
                    }

                    if (lastChar == 'm') {
                        if (decimal.TryParse(value.Remove(value.Length - 1), out decimal fVal)) {
                            return Expression.Constant(fVal);
                        }

                        throw new CompileException($"Tried to parse value {literalNode.rawValue} as a decimal but failed");
                    }

                    if (lastChar == 'u') {
                        if (uint.TryParse(value.Remove(value.Length - 1), out uint fVal)) {
                            return Expression.Constant(fVal);
                        }

                        throw new CompileException($"Tried to parse value {literalNode.rawValue} as a uint but failed");
                    }

                    if (lastChar == 'l') {
                        if (value.Length >= 2) {
                            char prevToLast = char.ToLower(value[value.Length - 2]);
                            if (prevToLast == 'u') {
                                if (ulong.TryParse(value.Remove(value.Length - 1), out ulong ulongVal)) {
                                    return Expression.Constant(ulongVal);
                                }

                                throw new CompileException($"Tried to parse value {literalNode.rawValue} as a ulong but failed");
                            }
                        }

                        if (long.TryParse(value.Remove(value.Length - 1), out long fVal)) {
                            return Expression.Constant(fVal);
                        }

                        throw new CompileException($"Tried to parse value {literalNode.rawValue} as a long but failed");
                    }

                    // no character specifier, parse as double if there is a decimal or int if there is not

                    if (value.Contains(".")) {
                        if (double.TryParse(value, out double fVal)) {
                            return Expression.Constant(fVal);
                        }

                        throw new CompileException($"Tried to parse value {literalNode.rawValue} as a double but failed");
                    }
                    else {
                        if (int.TryParse(value, out int fVal)) {
                            return Expression.Constant(fVal);
                        }

                        throw new CompileException($"Tried to parse value {literalNode.rawValue} as a int but failed");
                    }
                }

                if (int.TryParse(value, out int intVal)) {
                    return Expression.Constant(intVal);
                }

                throw new CompileException($"Tried to parse value {literalNode.rawValue} as a int but failed");
            }

            if (targetType == typeof(float)) {
                if (float.TryParse(literalNode.rawValue.Replace("f", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException("Tried to parse value {literalNode.rawValue} as a float but failed");
                }
            }

            if (targetType == typeof(int)) {
                if (int.TryParse(literalNode.rawValue, out int f)) {
                    return Expression.Constant(f);
                }
                else if (float.TryParse(literalNode.rawValue.Replace("f", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float fVal)) {
                    return Expression.Constant((int) fVal);
                }

                throw new CompileException($"Unable to parse {literalNode.rawValue} as an int value");
            }

            if (targetType == typeof(double)) {
                if (double.TryParse(literalNode.rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a double but failed");
                }
            }

            if (targetType == typeof(short)) {
                if (short.TryParse(literalNode.rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out short f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a short but failed");
                }
            }

            if (targetType == typeof(ushort)) {
                if (ushort.TryParse(literalNode.rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out ushort f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a ushort but failed");
                }
            }

            if (targetType == typeof(byte)) {
                if (byte.TryParse(literalNode.rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out byte f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a byte but failed");
                }
            }

            if (targetType == typeof(sbyte)) {
                if (sbyte.TryParse(literalNode.rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out sbyte f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a sbyte but failed");
                }
            }

            if (targetType == typeof(long)) {
                if (long.TryParse(literalNode.rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out long f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a long but failed");
                }
            }

            if (targetType == typeof(uint)) {
                if (uint.TryParse(literalNode.rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out uint f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a uint but failed");
                }
            }

            if (targetType == typeof(ulong)) {
                if (ulong.TryParse(literalNode.rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out ulong f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a ulong but failed");
                }
            }

            if (targetType == typeof(char)) {
                if (char.TryParse(literalNode.rawValue, out char f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a char but failed");
                }
            }

            if (targetType == typeof(decimal)) {
                if (decimal.TryParse(literalNode.rawValue, out decimal f)) {
                    return Expression.Constant(f);
                }
                else {
                    throw new CompileException($"Tried to parse value {literalNode.rawValue} as a decimal but failed");
                }
            }

            throw new CompileException($"Unable to parse numeric value from {literalNode.rawValue} target type was {targetType}");
        }

    }

}