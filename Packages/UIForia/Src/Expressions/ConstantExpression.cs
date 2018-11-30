using System;

namespace UIForia {

    public class ConstantExpression<T> : Expression<T> {

        private readonly T value;

        public ConstantExpression(T value) {
            this.value = value;
        }
        
        public override Type YieldedType => typeof(T);
        
        public override T Evaluate(ExpressionContext context) {
            return value;
        }

        public override bool IsConstant() {
            return true;
        }

    }

}