using System;

namespace Src {

    public class ResolveExpression_Alias<T> : Expression<T> {

        private readonly string alias;

        public ResolveExpression_Alias(string alias) {
            this.alias = alias;
        }

        public override Type YieldedType => typeof(T);

        public override T EvaluateTyped(ExpressionContext context) {
            return (T)context.ResolveRuntimeAlias(alias);
        }

        public override object Evaluate(ExpressionContext context) {
            return context.ResolveRuntimeAlias(alias);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}