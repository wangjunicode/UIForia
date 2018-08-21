using System;

namespace Src {

    public class LiteralExpression_Enum<T> : Expression<T> where T : IConvertible {

        private readonly T value;
        private readonly object boxedValue;

        public LiteralExpression_Enum(T value) {
            this.value = value;
            this.boxedValue = value;
        }

        public override Type YieldedType => value.GetType();

        public override T EvaluateTyped(ExpressionContext context) {
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