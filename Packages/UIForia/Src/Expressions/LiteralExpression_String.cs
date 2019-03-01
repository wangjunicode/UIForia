using System;

namespace UIForia.Expressions {

    public class LiteralExpression_String : Expression<string> {

        private readonly string value;

        public LiteralExpression_String(string value) {
            this.value = value;
        }

        public override string Evaluate(ExpressionContext context) {
            return value;
        }

        public override Type YieldedType => typeof(string);

        public override bool IsConstant() {
            return true;
        }

    }

}