using System;

namespace UIForia.Compilers.CastHandlers {

    public class CastHandler_FloatToMeasurement : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(UIMeasurement) && yieldedType == typeof(float);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<float> floatExpression = (Expression<float>) expression;

            return new CastExpression<float, UIMeasurement>(floatExpression,
                (measurementExpression, ctx) => { return new UIMeasurement(floatExpression.Evaluate(ctx)); });
        }

    }

}