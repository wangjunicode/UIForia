using System;

namespace Src {

    public class LiteralExpression_Int : Expression<int> {

        public readonly int value;
        public readonly object boxedValue;

        public LiteralExpression_Int(int value) {
            this.value = value;
            this.boxedValue = value;
        }

        public override Type YieldedType => typeof(int);

        public override int EvaluateTyped(ExpressionContext context) {
            return value;
        }

        public override object Evaluate(ExpressionContext context) {
            return boxedValue;
        }

    }

}