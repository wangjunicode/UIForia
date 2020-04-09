using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class CompilationContext2 {

        
        private static readonly MethodInfo s_Comment = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.Comment), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_InlineComment = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.InlineComment), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_CommentNewLineBefore = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.CommentNewLineBefore), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_CommentNewLineAfter = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.CommentNewLineAfter), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        
        public bool outputComments;
        private SizedArray<ParameterExpression> variables;
        private readonly LightList<Expression> statements;
        private List<ParameterExpression> parameters;
        private Type returnType;

        [ThreadStatic] private static List<ParameterExpression> parameterPool;

        public CompilationContext2() {
            outputComments = true;
            variables = new SizedArray<ParameterExpression>();
            statements = new LightList<Expression>();
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

            ParameterExpression param = GetParameter(type, name);
            variables.Add(param);
            return param;
        }
        
        public void AddStatement(Expression expression) {
            statements.Add(expression);
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

        public void Setup<T>() {
            returnType = typeof(T);
            parameters?.Clear();
            variables.Clear();
            statements.Clear();
        }
        
        public void Setup() {
            returnType = typeof(void);
            parameters?.Clear();
            variables.Clear();
            statements.Clear();
        }

        public LambdaExpression Build(string fnName) {
            return Expression.Lambda(Expression.Block(returnType, variables.CloneArray(), statements.ToArray()), fnName, false, parameters);
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