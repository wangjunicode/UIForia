using System;

namespace UIForia.Expressions {

  
    public abstract class Expression {

        public abstract Type YieldedType { get; }
        
        public abstract bool IsConstant();

    }

    public abstract class Expression<T> : Expression {

        public abstract T Evaluate(ExpressionContext context);

    }
    
}