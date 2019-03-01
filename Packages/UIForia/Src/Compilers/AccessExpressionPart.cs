using System;
using UIForia.Expressions;

namespace UIForia.Compilers {

    public abstract class AccessExpressionPart<TReturn, TInput> : AccessExpressionPart {

        public abstract TReturn Execute(TInput input, ExpressionContext context);

        public override Type YieldedType => typeof(TReturn);

    }

    public abstract class AccessExpressionPart {

        public abstract Type YieldedType { get; }

    }

}