using System;
using UIForia.Expressions;

namespace UIForia.Compilers.CastHandlers {

    public interface ICastHandler {

        bool CanHandle(Type requiredType, Type yieldedType);
        Expression Cast(Type requiredType, Expression expression);

    }

}