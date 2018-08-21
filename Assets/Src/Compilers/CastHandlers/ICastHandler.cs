using System;

namespace Src.Compilers.CastHandlers {

    public interface ICastHandler {

        bool CanHandle(Type requiredType, Type yieldedType);
        Expression Cast(Type requiredType, Expression expression);

    }

}