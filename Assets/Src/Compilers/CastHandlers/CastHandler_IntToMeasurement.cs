using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_IntToMeasurement : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(UIMeasurement) && yieldedType == typeof(int);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<int> intExpression = (Expression<int>) expression;

            return new TypedCastExpression<int, UIMeasurement>(intExpression,
                (exp, ctx) => new UIMeasurement(exp.EvaluateTyped(ctx)));
        }

    }

}