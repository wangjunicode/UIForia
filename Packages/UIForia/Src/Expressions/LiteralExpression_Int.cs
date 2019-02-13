using System;

namespace UIForia {

    public class LiteralExpression_Int : Expression<int> {

        private readonly int value;
        private readonly object boxedValue;

        public LiteralExpression_Int(int value) {
            this.value = value;
            this.boxedValue = value;
        }

        public override Type YieldedType => typeof(int);

        public override int Evaluate(ExpressionContext context) {
            return value;
        }

        public override bool IsConstant() {
            return true;
        }

    }

}