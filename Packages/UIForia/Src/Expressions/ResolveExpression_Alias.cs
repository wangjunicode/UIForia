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
            // todo find a more elegant place for this that is still fast, maybe GetContextValue can be virtual?
            // maybe used hooks in the compiler to extend alias resolution?
            if (alias == "$element") return (T)context.current;
            context.GetContextValue(context.current, alias, out resolved);
            return resolved;
        }

        public override object Evaluate(ExpressionContext context) {
            T resolved;
            if (alias == "$element") return context.current;
            context.GetContextValue(context.current, alias, out resolved);
            return resolved;
        }

        public override bool IsConstant() {
            return false;
        }

    }

}