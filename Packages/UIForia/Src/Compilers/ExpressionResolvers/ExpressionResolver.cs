using System.Collections.Generic;
using System.Reflection;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

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
            throw new CompileException($"Tried to invoke alias {aliasName} as a dot expression, but this is not supported");
        }

        public virtual Expression CompileAsIndexExpression(CompilerContext context, ASTNode indexNode) {
            throw new CompileException($"Tried to invoke alias {aliasName} as an index expression, but this is not supported");
        }

        public virtual Expression CompileAsMethodExpression(CompilerContext context, List<ASTNode> parameters) {
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