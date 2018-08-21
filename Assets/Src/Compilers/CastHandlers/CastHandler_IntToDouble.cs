using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_IntToDouble : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(double) && yieldedType == typeof(int);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<int> intExpression = (Expression<int>) expression;
            return new TypedCastExpression<int, double>(intExpression, (exp, ctx) => (double) exp.EvaluateTyped(ctx));
        }

    }

}