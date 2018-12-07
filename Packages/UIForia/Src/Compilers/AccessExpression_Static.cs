using System;

namespace UIForia.Compilers {

    public class AccessExpression_Static<T, U> : Expression<T> {

        public override Type YieldedType => typeof(T);

        public AccessExpressionPart<T, U> headExpression;
        public readonly bool isConstant;
        
        public AccessExpression_Static(AccessExpressionPart<T, U> headExpression, bool isConstant) {
            this.headExpression = headExpression;
            this.isConstant = isConstant;
        }

        public override T Evaluate(ExpressionContext context) {
            return headExpression.Execute(default(U), context);
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

}