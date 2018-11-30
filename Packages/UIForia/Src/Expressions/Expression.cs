using System;

namespace UIForia {

    public interface IExpression<out T> {

        Type YieldedType { get; }
        T Evaluate(ExpressionContext context);

    }
    
    public abstract class Expression {

        public abstract Type YieldedType { get; }
        
        //public abstract object Evaluate(ExpressionContext context);

        public abstract bool IsConstant();

    }

    public abstract class Expression<T> : Expression, IExpression<T> {

        public abstract T Evaluate(ExpressionContext context);

    }
    
}