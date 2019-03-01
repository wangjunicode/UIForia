using System;
using System.Reflection;
using UIForia.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class OperatorOverloadExpression<U, T> : Expression<T> {

        public readonly Expression<U> expr0;
        public readonly Func<U, T> overload;

        public OperatorOverloadExpression(Expression<U> expr0, MethodInfo info) {
            this.expr0 = expr0;
            this.overload = (Func<U, T>) ReflectionUtil.GetDelegate(info);
        }

        public override Type YieldedType => typeof(T);

        public override T Evaluate(ExpressionContext context) {
            return overload(expr0.Evaluate(context));
        }

        public override bool IsConstant() {
            // todo might also require the pure attribute
            return expr0.IsConstant();
        }

    }

    public class OperatorOverloadExpression<U, V, T> : Expression<T> {

        public readonly Expression<U> expr0;
        public readonly Expression<V> expr1;
        public readonly Func<U, V, T> overload;

        public OperatorOverloadExpression(Expression<U> expr0, Expression<V> expr1, MethodInfo info) {
            this.expr0 = expr0;
            this.expr1 = expr1;
            this.overload = (Func<U, V, T>) ReflectionUtil.GetDelegate(info);
        }

        public override Type YieldedType => typeof(T);

        public override T Evaluate(ExpressionContext context) {
            return overload(expr0.Evaluate(context), expr1.Evaluate(context));
        }

        public override bool IsConstant() {
            // todo might also require the pure attribute
            return expr0.IsConstant() && expr1.IsConstant();
        }

    }

}