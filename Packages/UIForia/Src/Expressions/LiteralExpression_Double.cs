using System;

namespace UIForia.Expressions {

    public class LiteralExpression_Double : Expression<double> {

        public readonly double value;
        public readonly object boxedValue;

        public LiteralExpression_Double(double value) {
            this.value = value;
            this.boxedValue = value;
        }

        public override Type YieldedType => typeof(double);

        public override double Evaluate(ExpressionContext context) {
            return value;
        }

        public override bool IsConstant() {
            return true;
        }

    }

}