using System;

namespace Src {

    public class ConstantExpression<T> : Expression<T> {

        private readonly T value;

        public ConstantExpression(T value) {
            this.value = value;
        }
        
        public override Type YieldedType => typeof(T);
        
        public override object Evaluate(ExpressionContext context) {
            return value;
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return value;
        }

        public override bool IsConstant() {
            return true;
        }

    }

}