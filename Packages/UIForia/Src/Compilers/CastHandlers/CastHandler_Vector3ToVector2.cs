using System;
using UIForia.Expressions;
using UnityEngine;

namespace UIForia.Compilers.CastHandlers {

    public class CastHandler_Vector3ToVector2 : ICastHandler {

        public bool CanHandle(Type requiredType, Type yieldedType) {
            return requiredType == typeof(Vector3) && yieldedType == typeof(Vector2);
        }

        public Expression Cast(Type requiredType, Expression expression) {
            
            Expression<Vector3> vec3Expression = (Expression<Vector3>) expression;
            
            return new CastExpression<Vector3, Vector2>(vec3Expression, (exp, ctx) => {
                Vector3 vec3 = exp.Evaluate(ctx);
                return new Vector2(vec3.x, vec3.y);
            });
        }

    }

}