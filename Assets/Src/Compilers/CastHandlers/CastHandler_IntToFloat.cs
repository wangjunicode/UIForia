using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_IntToFloat : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(float) && yieldedType == typeof(int);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<int> intExpression = (Expression<int>) expression;
            return new TypedCastExpression<int, float>(intExpression, (exp, ctx) => (float) exp.EvaluateTyped(ctx));
        }

    }

}