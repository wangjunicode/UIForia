using System;

namespace UIForia.Compilers {

    public class AccessExpression_Static<T, U> : Expression<T> {

        public override Type YieldedType => typeof(T);

        public AccessExpressionPart<T, U> headExpression;

        public AccessExpression_Static(AccessExpressionPart<T, U> headExpression) {
            this.headExpression = headExpression;
        }

        public override T Evaluate(ExpressionContext context) {
            return headExpression.Execute(default(U), context);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}