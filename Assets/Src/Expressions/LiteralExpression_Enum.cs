using System;

namespace Src {

    public class LiteralExpression_Enum : Expression<Enum> {

        private readonly Enum value;
        private readonly object boxedValue;

        public LiteralExpression_Enum(Enum value) {
            this.value = value;
            this.boxedValue = value;
        }

        public override Type YieldedType => value.GetType();

        public override Enum EvaluateTyped(ExpressionContext context) {
            return value;
        }

        public override object Evaluate(ExpressionContext context) {
            return boxedValue;
        }

        public override bool IsConstant() {
            return true;
        }

    }

}