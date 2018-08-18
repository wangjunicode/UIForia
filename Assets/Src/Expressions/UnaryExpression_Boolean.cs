using System;

namespace Src {

    public class UnaryExpression_Boolean : Expression<bool> {

        private readonly Expression<bool> expression;

        public UnaryExpression_Boolean(Expression<bool> expression) {
            this.expression = expression;
        }
        
        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            return !(expression.EvaluateTyped(context));
        }

        public override object Evaluate(ExpressionContext context) {
            object value = expression.Evaluate(context);
            if (value is bool) return !((bool) value);
            return value != null;
        }
       

    }

}