using System;
using UIForia.Expressions;

namespace UIForia.Compilers.CastHandlers {

    public class CastHandler_IntToDouble : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(double) && yieldedType == typeof(int);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<int> intExpression = (Expression<int>) expression;
            return new CastExpression<int, double>(intExpression, (exp, ctx) => (double) exp.Evaluate(ctx));
        }

    }

}