using System;
using System.Reflection;

namespace UIForia.Compilers {

    public class ImplicitCastExpression<T, U> : Expression<T> {

        public readonly Expression<U> expr;
        public readonly Func<U, T> converter;

        public ImplicitCastExpression(Expression<U> expr, MethodInfo info) {
            this.expr = expr;
            this.converter = (Func<U, T>) ReflectionUtil.GetDelegate(info);
        }

        public override Type YieldedType => typeof(T);

        public override T Evaluate(ExpressionContext context) {
            return converter(expr.Evaluate(context));
        }

        public override bool IsConstant() {
            return expr.IsConstant();
        }

    }

}