using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine.Assertions;

namespace UIForia.Compilers {

    public class CompilationContext2 {

        public bool outputComments;

        public ProcessedType rootType;
        public CompiledTemplate2 result;

        public readonly ParameterExpression systemParam;
        public ParameterExpression parentParam;
        public ParameterExpression elementParam;
        public ParameterExpression rootParam;
        public ParameterExpression dataParam;

        private static readonly MethodInfo s_Comment = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.Comment), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_InlineComment = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.InlineComment), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_CommentNewLineBefore = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.CommentNewLineBefore), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_CommentNewLineAfter = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.CommentNewLineAfter), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly char[] s_SplitChar = {'.'};
        public TemplateRootNode templateRootNode;
        public LightList<string> namespaces;
        private SizedArray<ParameterExpression> variables;
        private readonly LightStack<LightList<Expression>> statementStacks;
        private readonly LightStack<ParameterExpression> hierarchyStack;
        private int currentDepth;
        private int maxDepth;

        public SizedArray<ContextVariableDefinition> contextStack;
        
        public CompilationContext2() {
            outputComments = true;
            systemParam = Expression.Parameter(typeof(ElementSystem), "system");
            variables = new SizedArray<ParameterExpression>();
            statementStacks = new LightStack<LightList<Expression>>();
            statementStacks.Push(new LightList<Expression>());
        }

        public void PushScope() {
            currentDepth++;

            if (currentDepth > maxDepth) {
                maxDepth = currentDepth;
                ParameterExpression variable = Expression.Parameter(typeof(UIElement), "targetElement_" + currentDepth);
                variables.Add(variable);
                hierarchyStack.Push(variable);
            }
            else {
                string targetName = "targetElement_" + currentDepth;
                for (int i = 0; i < variables.size; i++) {
                    if (variables[i].Type == typeof(UIElement) && variables[i].Name == targetName) {
                        hierarchyStack.Push(variables[i]);
                        return;
                    }
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        // todo -- pool variables where possible
        public ParameterExpression GetVariable(Type type, string name) {
            for (int i = 0; i < variables.size; i++) {
                if (variables[i].Name == name) {
                    if (variables[i].Type != type) {
                        throw new TemplateCompileException("Variable already taken: " + name);
                    }

                    return variables[i];
                }
            }

            ParameterExpression param = Expression.Parameter(type, name);
            variables.Add(param);
            return param;
        }

        public ParameterExpression GetVariable<T>(string name) {
            for (int i = 0; i < variables.size; i++) {
                if (variables[i].Name == name) {
                    if (variables[i].Type != typeof(T)) {
                        throw new TemplateCompileException("Variable already taken: " + name);
                    }

                    return variables[i];
                }
            }

            ParameterExpression param = Expression.Parameter(typeof(T), name);
            variables.Add(param);
            return param;
        }

        public void PopScope() {
            currentDepth--;
            hierarchyStack.Pop();
        }

        public void PushBlock() {
            statementStacks.Push(LightList<Expression>.Get());
        }

        public BlockExpression PopBlock() {
            LightList<Expression> statements = statementStacks.Pop();
            Expression[] array = statements.ToArray();
            LightList<Expression>.Release(ref statements);
            return Expression.Block(typeof(void), array);
        }

        public LambdaExpression Finalize(Type type) {
            LightList<Expression> statements = statementStacks.Pop();
            Expression[] array = statements.ToArray();
            LightList<Expression>.Release(ref statements);
            return Expression.Lambda(Expression.Block(type, variables.CloneArray(), array), systemParam, rootParam, parentParam, elementParam, dataParam);
        }

        public void AddStatement(Expression expression) {
            this.statementStacks.PeekUnchecked().Add(expression);
        }

        public void Assign(Expression target, Expression value) {
            AddStatement(Expression.Assign(target, value));
        }

        public void Return(Expression arg) {
            AddStatement(arg);
        }

        public void IfEqualsNull(Expression target, BlockExpression block) {
            AddStatement(Expression.IfThen(Expression.Equal(target, Expression.Constant(null)), block));
        }

        public void Comment(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_Comment, Expression.Constant(comment)));
            }
        }

        public void CommentNewLineBefore(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_CommentNewLineBefore, Expression.Constant(comment)));
            }
        }

        public void CommentNewLineAfter(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_CommentNewLineAfter, Expression.Constant(comment)));
            }
        }

        public void InlineComment(string comment) {
            if (outputComments) {
                AddStatement(ExpressionFactory.CallStaticUnchecked(s_InlineComment, Expression.Constant(comment)));
            }
        }

        public void Clear() {
            statementStacks.Clear();
            variables.Clear();
        }

        public void SetupHydrate() {
            throw new NotImplementedException();
        }

        public void Setup<T>() {
            returnType = typeof(T);
            parameters?.Clear();
            variables.Clear();
        }
        
        public void Setup() {
            returnType = typeof(void);
            parameters?.Clear();
            variables.Clear();
        }

        private List<ParameterExpression> parameters;
        private Type returnType;

        [ThreadStatic] private static List<ParameterExpression> parameterPool;

        public LambdaExpression Build(string fnName) {
            
            Assert.IsTrue(statementStacks.size == 1);
            
            LightList<Expression> statements = statementStacks.array[0];
            Expression[] array = statements.ToArray();
            statements.Clear();
            return Expression.Lambda(Expression.Block(returnType, variables.CloneArray(), array), fnName, false, parameters);
        }
        
        private static ParameterExpression GetParameter(Type type, string name) {
            parameterPool = parameterPool ?? new List<ParameterExpression>(12);
            for (int i = 0; i < parameterPool.Count; i++) {
                if (parameterPool[i].Type == type && parameterPool[i].Name == name) {
                    return parameterPool[i];
                }
            }

            ParameterExpression parameter = Expression.Parameter(type, name);
            parameterPool.Add(parameter);
            return parameter;
        }

        public ParameterExpression AddParameter<T>(string name) {
            parameters = parameters ?? new List<ParameterExpression>(4);
            ParameterExpression retn = GetParameter(typeof(T), name);
            parameters.Add(retn);
            return retn;
        }

        public ParameterExpression this[string name] {
            get {
                for (int i = 0; i < parameters.Count; i++) {
                    if (parameters[i].Name == name) {
                        return parameters[i];
                    }
                }

                return null;
            }
        }
    }

}