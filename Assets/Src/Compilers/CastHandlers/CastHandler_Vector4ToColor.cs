using System;
using UnityEngine;

namespace Src.Compilers.CastHandlers {

    public class CastHandler_Vector4ToColor : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(Color) && yieldedType == typeof(Vector4);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<Vector4> vec4Expression = (Expression<Vector4>) expression;
            return new TypedCastExpression<Vector4, Color>(vec4Expression,
                (exp, ctx) => { return (Color) exp.EvaluateTyped(ctx); });
        }

    }

}