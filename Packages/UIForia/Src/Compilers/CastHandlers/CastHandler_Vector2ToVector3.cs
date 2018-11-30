using System;
using UnityEngine;

namespace UIForia.Compilers.CastHandlers {

    public class CastHandler_Vector2ToVector3 : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(Vector2) && yieldedType == typeof(Vector3);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            Expression<Vector2> vec2Expression = (Expression<Vector2>) expression;
            return new CastExpression<Vector2, Vector3>(vec2Expression, (exp, ctx) => {
                Vector2 vec2 = exp.Evaluate(ctx);
                return new Vector3(vec2.x, vec2.y, 0);
            });
        }

    }

}