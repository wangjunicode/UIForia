using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_DoubleToFixedLength : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(UIFixedLength) && yieldedType == typeof(double);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<double> doubleExpression = (Expression<double>) expression;

            return new CastExpression<double, UIFixedLength>(doubleExpression, (exp, ctx) => new UIFixedLength((float)exp.EvaluateTyped(ctx)));
        }

    }

}