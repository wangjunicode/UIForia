using System;

namespace Src {

    public class CastExpression<T> : Expression<T> {

        public readonly Expression expression;
        public readonly Func<Expression, ExpressionContext, T> fn;
        
        public CastExpression(Expression expression, Func<Expression, ExpressionContext, T> fn) {
            this.fn = fn;
            this.expression = expression;
        }
        
        public override T EvaluateTyped(ExpressionContext context) {
            return fn(expression, context);
        }

        public override Type YieldedType => typeof(T);
        
        public override object Evaluate(ExpressionContext context) {
            return fn(expression, context);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}