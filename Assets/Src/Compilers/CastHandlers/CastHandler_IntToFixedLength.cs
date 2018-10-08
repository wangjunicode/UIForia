using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_IntToFixedLength : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(UIFixedLength) && yieldedType == typeof(int);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<int> intExpression = (Expression<int>) expression;

            return new CastExpression<int, UIFixedLength>(intExpression, (exp, ctx) => new UIFixedLength(exp.EvaluateTyped(ctx)));
        }

    }

}