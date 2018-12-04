using System;
using System.Reflection;
using UIForia.Parsing;

namespace UIForia.Compilers {

    public abstract class ExpressionAliasResolver {

        public string aliasName;

        public ExpressionAliasResolver(string aliasName) {
            if (aliasName[0] != '$') {
                aliasName = "$" + aliasName;
            }

            this.aliasName = aliasName;
        }

        public virtual Expression CompileAsValueExpression2(IdentifierNode node, Func<ASTNode, Expression> visit) {
            return null;
        }
        
        public virtual Expression CompileAsValueExpression(ASTNode node, Func<ASTNode, Expression> visit) {
            return null;
        }

        public virtual Expression CompileAsAccessExpression(ASTNode node, Func<ASTNode, Expression> visit) {
            return null;
        }

        public virtual Expression CompileAsMethodExpression(MethodCallNodeOld node, Func<ASTNode, Expression> visit) {
            return null;
        }

    }

    public class ValueResolver<T> : ExpressionAliasResolver {

        public readonly T value;
        
        public ValueResolver(string aliasName, T value) : base(aliasName) {
            this.value = value;
        }

        public override Expression CompileAsValueExpression(ASTNode node, Func<ASTNode, Expression> visit) {
            return new ConstantExpression<T>(value);
        }

    }

    public class MethodResolver : ExpressionAliasResolver {

        public readonly MethodInfo info;

        public MethodResolver(string aliasName, MethodInfo info) : base(aliasName) {
            this.info = info;
        }

        public override Expression CompileAsValueExpression(ASTNode node, Func<ASTNode, Expression> visit) {
            return null;
        }

    }

    
    public class EnumResolver<T> : ExpressionAliasResolver {

        public EnumResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsAccessExpression(ASTNode node, Func<ASTNode, Expression> visit) {
            return new ConstantExpression<T>(default);
        }

        public class EnumValueExpression : Expression<T> {

            public override Type YieldedType => typeof(T);
            
            public override T Evaluate(ExpressionContext context) {
                throw new NotImplementedException();
            }

            public override bool IsConstant() {
                return true;
            }

        }

    }
}