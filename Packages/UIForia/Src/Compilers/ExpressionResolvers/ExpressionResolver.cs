using System;
using System.Reflection;

namespace UIForia.Compilers {

    public abstract class ExpressionAliasResolver {

        public string aliasName;

        public ExpressionAliasResolver(string aliasName) {
            if (aliasName[0] != '$') {
                aliasName = "$" + aliasName;
            }

            this.aliasName = aliasName;
        }

        public virtual Expression CompileAsValueExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            return null;
        }

        public virtual Expression CompileAsAccessExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            return null;
        }

        public virtual Expression CompileAsMethodExpression(MethodCallNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            return null;
        }

    }

    public class ValueResolver<T> : ExpressionAliasResolver {

        public readonly T value;
        
        public ValueResolver(string aliasName, T value) : base(aliasName) {
            this.value = value;
        }

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            return new ConstantExpression<T>(value);
        }

    }

    public class MethodResolver : ExpressionAliasResolver {

        public readonly MethodInfo info;

        public MethodResolver(string aliasName, MethodInfo info) : base(aliasName) {
            this.info = info;
        }

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            return null;
        }

    }

    
    public class EnumResolver<T> : ExpressionAliasResolver {

        public EnumResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsAccessExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {

            AccessExpressionNodeOld x = nodeOld as AccessExpressionNodeOld;
            if (x.parts.Count != 1) {
                return null;
            }
            
            return new ConstantExpression<T>(default);
        }

        public class EnumValueExpression : Expression<T> {

            public override Type YieldedType => typeof(T);
            
            public override object Evaluate(ExpressionContext context) {
                throw new NotImplementedException();
            }

            public override T EvaluateTyped(ExpressionContext context) {
                throw new NotImplementedException();
            }

            public override bool IsConstant() {
                return true;
            }

        }

    }
}