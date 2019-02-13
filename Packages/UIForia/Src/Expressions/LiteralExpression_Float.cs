using System;

namespace UIForia {

    public class LiteralExpression_Float : Expression<float> {

        public readonly float value;
        public readonly object boxedValue;

        public LiteralExpression_Float(float value) {
            this.value = value;
            this.boxedValue = value;
        }

        public override Type YieldedType => typeof(float);

        public override float Evaluate(ExpressionContext context) {
            return value;
        }

        public override bool IsConstant() {
            return true;
        }

    }

}