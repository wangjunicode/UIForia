using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_FloatToFixedLength : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(UIFixedLength) && yieldedType == typeof(float);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<float> floatExpression = (Expression<float>) expression;

            return new CastExpression<float, UIFixedLength>(floatExpression, (measurementExpression, ctx) => new UIFixedLength(floatExpression.EvaluateTyped(ctx)));
        }

    }

}