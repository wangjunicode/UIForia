using System;
using UnityEngine;

namespace UIForia.Compilers.CastHandlers {

    public class CastHandler_ColorToVector4 : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(Vector4) && yieldedType == typeof(Color);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<Color> colorExpression = (Expression<Color>) expression;
            return new CastExpression<Color, Vector4>(colorExpression,
                (exp, ctx) => { return (Vector4) exp.Evaluate(ctx); });
        }

    }

}