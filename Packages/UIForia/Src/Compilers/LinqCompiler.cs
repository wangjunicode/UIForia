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

        public LinqCompiler() {
            this.parameters = new List<ParameterExpression>();
            this.blockStack = new LightStack<BlockDefinition>();
            blockStack.Push(new BlockDefinition());
        }

        private BlockDefinition currentBlock => blockStack.Peek();

        public void Reset() {
            this.parameters.Clear();
            this.blockStack.Clear();
            blockStack.Push(new BlockDefinition());
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
            return Expression.Lambda(currentBlock.ToExpressionBlock(typeof(void)), parameters.ToArray());
        }

        public T Compile<T>() where T : Delegate {
            Debug.Assert(blockStack.Count == 1);
            Expression<T> expr;
            Type genericTypeDef = typeof(T).GetGenericTypeDefinition();
            if (genericTypeDef == typeof(Func<>)) {
                Type[] genericArguments = typeof(T).GetGenericArguments();
                expr = Expression.Lambda<T>(currentBlock.ToExpressionBlock(genericArguments[genericArguments.Length - 1]), parameters.ToArray());
            }
            else {
                expr = Expression.Lambda<T>(currentBlock.ToExpressionBlock(typeof(void)), parameters.ToArray());
            }
            return expr.Compile();
        }

        public ParameterExpression AddParameter(Type type, string name) {
            // todo validate no name conflicts
            ParameterExpression retn = Expression.Parameter(type, name);
            parameters.Add(retn);
            return retn;
        }

        public class LinqAliasResolver { }


        public BindingFlags k_DefaultBindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static Type GetMemberType(Type type, string memberName) {
            if (ReflectionUtil.IsField(type, memberName, out FieldInfo fieldInfo)) {
                return fieldInfo.FieldType;
            }

            if (ReflectionUtil.IsProperty(type, memberName, out PropertyInfo propertyInfo)) {
                return propertyInfo.PropertyType;
            }

            return null;
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
                // StructValueHolder<Vector3> lhs;
                // Vector3 lhs0;

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
                        //if (i != memberNode.parts.Count - 1) {
                        currentBlock.AddStatement(Expression.Assign(variable, last));
                        // }

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

        private void CompileAccessExpression(RHSStatementChain retn, string rootVariableName, Type targetType, MemberAccessExpressionNode accessNode) {
            List<ASTNode> parts = accessNode.parts;

            // assume not an alias for now
            ParameterExpression head = ResolveVariableName(rootVariableName);

            Expression last = MemberAccess(head, accessNode.identifier);
            bool needsNullChecking = false;

            LabelTarget returnTarget = Expression.Label("retn");

            ParameterExpression outputVariable = currentBlock.AddVariable(targetType, "rhsOutput");
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
                        currentBlock.AddStatement(NullCheck(lastValue, outputVariable, returnTarget));
                    }

                    last = MemberAccess(lastValue, dotAccessNode.propertyName);

                    // todo -- optimization would be to do this while the next part is dot access & a struct so we don't over copy    

                    lastValue = currentBlock.AddVariable(last.Type, "part" + i);
                    currentBlock.AddStatement(Expression.Assign(lastValue, last));
                }
                else if (parts[i] is IndexNode indexNode) {
                    Type lastValueType = lastValue.Type;
                    // todo -- also no support for multiple index properties right now, parser needs to accept a comma list for that to work

                    if (lastValueType.IsArray) {
                        needsNullChecking = true;

                        if (lastValueType.GetArrayRank() != 1) {
                            throw new NotSupportedException("Expressions do not support multidimensional arrays yset");
                        }

                        Expression indexExpression = Visit(typeof(int), indexNode.expression);
                        Expression indexer = currentBlock.AddVariable(indexExpression.Type, "indexer");
                        currentBlock.AddStatement(Expression.Assign(indexer, indexExpression));
                        currentBlock.AddStatement(NullAndBoundsCheck(indexer, lastValue, "Length", outputVariable, returnTarget));

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
                            currentBlock.AddStatement(NullAndBoundsCheck(indexer, lastValue, "Count", outputVariable, returnTarget));
                        }
                        else if (lastValueType.IsClass) {
                            needsNullChecking = true;
                            currentBlock.AddStatement(NullCheck(lastValue, outputVariable, returnTarget));
                        }

                        last = Expression.MakeIndex(lastValue, indexProperty, new[] {indexer});
                        lastValue = currentBlock.AddVariable(indexProperty.PropertyType, "indexVal");
                        currentBlock.AddStatement(Expression.Assign(lastValue, last));
                    }
                }
            }

            // todo -- can remove last statement and assign directly to outputVariable (also remove the variable)
            currentBlock.AddStatement(Expression.Assign(outputVariable, lastValue));

            if (needsNullChecking) {
                currentBlock.AddStatement(Expression.Label(returnTarget));
            }

            retn.OutputExpression = outputVariable;
        }

        private Expression NullAndBoundsCheck(Expression indexExpression, Expression variable, string field, ParameterExpression outputVariable, LabelTarget returnTarget) {
            if (variable.Type.IsClass) {
                return Expression.IfThen(
                    Expression.OrElse(
                        Expression.Equal(variable, Expression.Constant(null)),
                        Expression.OrElse(
                            Expression.LessThan(indexExpression, Expression.Constant(0)),
                            Expression.GreaterThanOrEqual(indexExpression, MemberAccess(variable, field))
                        )),
                    Expression.Block(typeof(void),
                        Expression.Assign(outputVariable, Expression.Default(outputVariable.Type)),
                        Expression.Goto(returnTarget)
                    )
                );
            }
            else {
                return Expression.IfThen(
                    Expression.OrElse(
                        Expression.LessThan(indexExpression, Expression.Constant(0)),
                        Expression.GreaterThanOrEqual(indexExpression, MemberAccess(variable, field))
                    ),
                    Expression.Block(typeof(void),
                        Expression.Assign(outputVariable, Expression.Default(outputVariable.Type)),
                        Expression.Goto(returnTarget)
                    )
                );
            }
        }

        private static Expression NullCheck(Expression variable, Expression output, LabelTarget label) {
            return Expression.IfThen(
                Expression.Equal(variable, Expression.Constant(null)),
                Expression.Block(typeof(void),
                    Expression.Assign(output, Expression.Default(output.Type)),
                    Expression.Goto(label)
                )
            );
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

        private Expression Visit(Type targetType, ASTNode node) {
            switch (node.type) {
                case ASTNodeType.NullLiteral:
                    return Expression.Constant(null);

                case ASTNodeType.BooleanLiteral:
                    return VisitBoolLiteral((LiteralNode) node);

                case ASTNodeType.NumericLiteral:
                    return VisitNumericLiteral(targetType, (LiteralNode) node);

                case ASTNodeType.DefaultLiteral:
                    return Expression.Default(targetType);

                case ASTNodeType.StringLiteral:
                    return Expression.Constant(((LiteralNode) node).rawValue);

                case ASTNodeType.Operator:
                    break;

                case ASTNodeType.TypeOf:
                    break;
                case ASTNodeType.Identifier:
                    break;
                case ASTNodeType.DotAccess:
                    break;
                case ASTNodeType.AccessExpression:
                    break;
                case ASTNodeType.IndexExpression:
                    break;
                case ASTNodeType.UnaryNot:
                    break;
                case ASTNodeType.UnaryMinus:
                    break;
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

                default:
                    throw new CompileException($"Tried to visit the operator node {operatorNode.operatorType} but it wasn't handled by LinqCompiler.VisitOperator");
            }
        }

        public RHSStatementChain CreateRHSStatementChain(string input) {
            return CreateRHSStatementChain(null, null, input);
        }

        public RHSStatementChain CreateRHSStatementChain(string rootVariableName, Type targetType, string input) {
            ASTNode astRoot = ExpressionParser.Parse(input);

            RHSStatementChain retn = new RHSStatementChain();

            switch (astRoot.type) {
                case ASTNodeType.NullLiteral:
                    retn.OutputExpression = Expression.Constant(null);
                    break;

                case ASTNodeType.BooleanLiteral:
                    retn.OutputExpression = VisitBoolLiteral((LiteralNode) astRoot);
                    break;

                case ASTNodeType.NumericLiteral: {
                    retn.OutputExpression = VisitNumericLiteral(targetType, (LiteralNode) astRoot);
                    break;
                }

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
                    break;

                case ASTNodeType.Identifier: {
                    if (string.IsNullOrEmpty(rootVariableName) || string.IsNullOrWhiteSpace(rootVariableName)) {
                        throw CompileException.RHSRootIdentifierMissing(((IdentifierNode) astRoot).name);
                    }

                    retn.OutputExpression = VisitIdentifierNode(rootVariableName, (IdentifierNode) astRoot);
                    break;
                }

                case ASTNodeType.AccessExpression: {
                    CompileAccessExpression(retn, rootVariableName, targetType, (MemberAccessExpressionNode) astRoot);
                    break;
                }

                case ASTNodeType.IndexExpression:
                    break;

                case ASTNodeType.UnaryNot:
                    break;

                case ASTNodeType.UnaryMinus:
                    break;

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

            return retn;
        }

        private Expression VisitIdentifierNode(string targetName, IdentifierNode identifierNode) {
            if (identifierNode.IsAlias) {
                throw new NotImplementedException("Aliases aren't support yet");
            }

            Expression expr = MemberAccess(ResolveVariableName(targetName), identifierNode.name);
            Expression variable = currentBlock.AddVariable(expr.Type, identifierNode.name);
            currentBlock.AddStatement(Expression.Assign(variable, expr));
            return variable;
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