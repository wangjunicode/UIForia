using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_FloatToInt : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(int) && yieldedType == typeof(float);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<float> floatExpression = (Expression<float>) expression;
            return new TypedCastExpression<float, int>(floatExpression, (exp, ctx) => (int) exp.EvaluateTyped(ctx));
        }

    }

}