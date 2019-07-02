using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UIForia.Exceptions;
using UIForia.Expressions;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Compilers.ExpressionResolvers {

    public abstract class ExpressionAliasResolver {

        public string aliasName;

        protected ExpressionAliasResolver(string aliasName) {
            if (aliasName[0] != '$') {
                aliasName = "$" + aliasName;
            }

            this.aliasName = aliasName;
        }

        public virtual Expression CompileAsValueExpression(CompilerContext context) {
            throw new CompileException($"Tried to invoke alias {aliasName} as a value in expression, but this is not supported");
        }

        public virtual Expression CompileAsDotExpression(CompilerContext context, string propertyName) {
            Expression expression = CompileAsValueExpression(context);
            if (expression != null) {
                try {
                    return context.compiler.CompileRestOfChain(expression, context);
                }
                catch (Exception e) {
                    Debug.LogError(e);
                    throw new CompileException($"Cannot resolve property {propertyName} of alias {aliasName}.");
                }
            }
            throw new CompileException($"Tried to invoke alias {aliasName} as a dot expression, but this is not supported");
        }

        public virtual Expression CompileAsIndexExpression(CompilerContext context, ASTNode indexNode) {
            Expression expression = CompileAsValueExpression(context);
            if (expression != null) {
                return context.compiler.CompileRestOfChain(expression, context);
            }
            throw new CompileException($"Tried to invoke alias {aliasName} as an index expression, but this is not supported");
        }

        public virtual Expression CompileAsMethodExpression(CompilerContext context, LightList<ASTNode> parameters) {
            Expression expression = CompileAsValueExpression(context);
            if (expression != null) {
                return context.compiler.CompileRestOfChain(expression, context);
            }
            throw new CompileException($"Tried to invoke alias {aliasName} as a method expression, but this is not supported");
        }

    }

    public class ValueResolver<T> : ExpressionAliasResolver {

        public readonly T value;

        public ValueResolver(string aliasName, T value) : base(aliasName) {
            this.value = value;
        }

//        public override Expression CompileAsValueExpression(ASTNode node, Func<Type, ASTNode, Expression> visit) {
//            return new ConstantExpression<T>(value);
//        }

    }

    public class MethodResolver : ExpressionAliasResolver {

        public readonly LightList<MethodInfo> infos;

        public MethodResolver(string aliasName, LightList<MethodInfo> infos) : base(aliasName) {
            this.infos = infos;
        }

        public MethodResolver(string aliasName, MethodInfo info) : base(aliasName) {
            this.infos = new LightList<MethodInfo>(1);
            this.infos.Add(info);
        }

    }


    public class EnumResolver<T> : ExpressionAliasResolver {

        public EnumResolver(string aliasName) : base(aliasName) { }

//        public override Expression CompileAsAccessExpression(ASTNode node, Func<Type, ASTNode, Expression> visit) {
//            return new ConstantExpression<T>(default);
//        }
//
//        public class EnumValueExpression : Expression<T> {
//
//            public override Type YieldedType => typeof(T);
//            
//            public override T Evaluate(ExpressionContext context) {
//                throw new NotImplementedException();
//            }
//
//            public override bool IsConstant() {
//                return true;
//            }
//
//        }

    }

}