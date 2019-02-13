using System;

namespace UIForia.Compilers {

    public class AccessExpression<T, U> : Expression<T> {

        public override Type YieldedType => typeof(T);

        public AccessExpressionPart<T, U> headExpression;

        public AccessExpression(AccessExpressionPart<T, U> headExpression) {
            this.headExpression = headExpression;
        }

        public override T Evaluate(ExpressionContext context) {
            return headExpression.Execute((U) context.rootObject, context);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}