using System;

namespace Src {

    public class ResolveExpression_Alias_Int : Expression<int> {

        public readonly string alias;

        public ResolveExpression_Alias_Int(string alias) {
            this.alias = alias;
        }

        public override Type YieldedType => typeof(int);

        public override int EvaluateTyped(ExpressionContext context) {
            return context.ResolveIntAlias(alias);
        }

        public override object Evaluate(ExpressionContext context) {
            return context.ResolveIntAlias(alias);
        }

        public override bool IsConstant() {
            return false;
        }

    }

    public class ResolveExpression_Alias_Object : Expression {

        public readonly string alias;
        public readonly Type yieldedType;

        public ResolveExpression_Alias_Object(string alias, Type yieldedType) {
            this.alias = alias;
            this.yieldedType = yieldedType;
        }

        public override Type YieldedType => yieldedType;

        public override object Evaluate(ExpressionContext context) {
            return context.ResolveObjectAlias(alias);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}