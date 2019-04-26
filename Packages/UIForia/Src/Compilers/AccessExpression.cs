using System;
using UIForia.Expressions;
using UnityEngine;

namespace UIForia.Compilers {

    public class AccessExpression<T, U> : Expression<T> {

        public override Type YieldedType => typeof(T);

        public AccessExpressionPart<T, U> headExpression;

        public AccessExpression(AccessExpressionPart<T, U> headExpression) {
            this.headExpression = headExpression;
        }

        public override T Evaluate(ExpressionContext context) {
            if (context.rootObject is U input) {
                return headExpression.Execute(input, context);
            }
    
            Debug.Log($"Casing from {context.rootObject.GetType()} to {typeof(U)} didn't work. Expression did not execute.");
            return default;
        }

        public override bool IsConstant() {
            return false;
        }

    }

}