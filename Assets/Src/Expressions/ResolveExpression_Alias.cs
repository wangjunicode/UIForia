using System;

namespace UIForia {
    
    public class ResolveExpression_Alias<T> : Expression<T> {

        private readonly string alias;

        public ResolveExpression_Alias(string alias) {
            this.alias = alias;
        }

        public override Type YieldedType => typeof(T);

        public override T EvaluateTyped(ExpressionContext context) {
            T resolved;
            context.GetContextValue(context.current, alias, out resolved);
            return resolved;
        }

        public override object Evaluate(ExpressionContext context) {
            T resolved;
            context.GetContextValue(context.current, alias, out resolved);
            return resolved;
        }

        public override bool IsConstant() {
            return false;
        }

    }

}