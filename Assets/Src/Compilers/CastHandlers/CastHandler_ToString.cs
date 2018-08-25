using System;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_ToString : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(string) && yieldedType != typeof(string);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            ReflectionUtil.ObjectArray1[0] = expression;
            return (Expression) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(StringCastExpression<>),
                expression.YieldedType,
                ReflectionUtil.ObjectArray1
            );
        }

    }

}