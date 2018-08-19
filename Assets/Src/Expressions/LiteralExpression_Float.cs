using System;

namespace Src {

    public class LiteralExpression_Float : Expression<float> {

        public readonly float value;
        public readonly object boxedValue;

        public LiteralExpression_Float(float value) {
            this.value = value;
            this.boxedValue = value;
        }

        public override Type YieldedType => typeof(float);

        public override float EvaluateTyped(ExpressionContext context) {
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