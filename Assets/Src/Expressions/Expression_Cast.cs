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
            return expression.IsConstant();
        }

    }
    
    public class TypedCastExpression<UFromType, TToType> : Expression<TToType> {

        private readonly Expression<UFromType> expression;
        private readonly Func<Expression<UFromType>, ExpressionContext, TToType> fn;
        
        public TypedCastExpression(Expression<UFromType> expression, Func<Expression<UFromType>, ExpressionContext, TToType> fn) {
            this.fn = fn;
            this.expression = expression;
        }
        
        public override TToType EvaluateTyped(ExpressionContext context) {
            return fn(expression, context);
        }

        public override Type YieldedType => typeof(TToType);
        
        public override object Evaluate(ExpressionContext context) {
            return fn(expression, context);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}