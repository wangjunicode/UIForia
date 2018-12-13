using System;

namespace UIForia.Compilers {

    public class AliasAccessBridge<TReturn, TInput, TNext> : AccessExpressionPart<TReturn, TInput> {

        private Expression<TNext> expr;
        public AccessExpressionPart<TReturn, TNext> headExpression;

        public AliasAccessBridge(Expression<TNext> expr, AccessExpressionPart<TReturn, TNext> headExpression) {
            this.expr = expr;
            this.headExpression = headExpression;
        }
            
        public override TReturn Execute(TInput input, ExpressionContext context) {
            return headExpression.Execute(expr.Evaluate(context), context);
        }

        public override Type YieldedType => expr.YieldedType;

    }

}