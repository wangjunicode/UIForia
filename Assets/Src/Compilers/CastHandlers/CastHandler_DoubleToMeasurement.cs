using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_DoubleToMeasurement : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(UIMeasurement) && yieldedType == typeof(double);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<double> doubleExpression = (Expression<double>) expression;

            return new CastExpression<double, UIMeasurement>(doubleExpression,
                (exp, ctx) => new UIMeasurement(exp.EvaluateTyped(ctx)));
        }

    }

}