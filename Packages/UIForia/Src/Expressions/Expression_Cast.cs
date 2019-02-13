using System;

namespace UIForia {

    public class StringCastExpression<T> : Expression<string> {

        private readonly Expression<T> expression;

        public StringCastExpression(Expression<T> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(string);

        public override string Evaluate(ExpressionContext context) {
            return expression.Evaluate(context)?.ToString();
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class CastExpression<UFromType, TToType> : Expression<TToType> {

        private readonly Expression<UFromType> expression;
        private readonly Func<Expression<UFromType>, ExpressionContext, TToType> fn;

        public CastExpression(Expression<UFromType> expression, Func<Expression<UFromType>, ExpressionContext, TToType> fn) {
            this.fn = fn;
            this.expression = expression;
        }

        public override Type YieldedType => typeof(TToType);

        public override TToType Evaluate(ExpressionContext context) {
            return fn(expression, context);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}