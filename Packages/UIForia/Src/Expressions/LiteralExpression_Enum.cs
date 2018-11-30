using System;

namespace UIForia {

    public class LiteralExpression_Enum<T> : Expression<T> where T : IConvertible {

        private readonly T value;
        private readonly object boxedValue;

        public LiteralExpression_Enum(T value) {
            this.value = value;
            this.boxedValue = value;
        }

        public override Type YieldedType => value.GetType();

        public override T Evaluate(ExpressionContext context) {
            return value;
        }

        public override bool IsConstant() {
            return true;
        }

    }

}