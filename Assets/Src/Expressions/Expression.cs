using System;

namespace Src {

    public interface IExpression<out T> {

        Type YieldedType { get; }
        object Evaluate(ExpressionContext context);
        T EvaluateTyped(ExpressionContext context);

    }
    
    public abstract class Expression {

        public abstract Type YieldedType { get; }
        
        public abstract object Evaluate(ExpressionContext context);

        public abstract bool IsConstant();

    }

    public abstract class Expression<T> : Expression, IExpression<T> {

        public abstract T EvaluateTyped(ExpressionContext context);

    }
    
}