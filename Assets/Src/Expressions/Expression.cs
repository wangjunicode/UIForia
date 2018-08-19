using System;

namespace Src {

    public abstract class Expression {

        public abstract Type YieldedType { get; }
        
        public abstract object Evaluate(ExpressionContext context);

        public abstract bool IsConstant();

        public Expression<T> Cast<T>(Func<Expression, ExpressionContext, T> fn) {
            return new CastExpression<T>(this, fn);
        }

    }

    public abstract class Expression<T> : Expression {

        public abstract T EvaluateTyped(ExpressionContext context);

    }
    
}