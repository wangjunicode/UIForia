using System;

namespace Src {

    public class OperatorExpression_Ternary : Expression {

        public readonly Expression left;
        public readonly Expression right;
        public readonly Expression<bool> condition;

        public OperatorExpression_Ternary(Expression<bool> condition, Expression left, Expression right) {
            this.condition = condition;
            this.left = left;
            this.right = right;
            YieldedType = ReflectionUtil.GetCommonBaseClass(left.YieldedType, right.YieldedType);
        }

        public override Type YieldedType { get; }

        public override object Evaluate(ExpressionContext context) {
            return condition.EvaluateTyped(context) ? left.Evaluate(context) : right.Evaluate(context);
        }

    }

    public class OperatorExpression_Ternary_Generic<T> : Expression<T> {

        public readonly Expression<T> left;
        public readonly Expression<T> right;
        public readonly Expression<bool> condition;

        public OperatorExpression_Ternary_Generic(Expression<bool> condition, Expression<T> left, Expression<T> right) {
            this.condition = condition;
            this.left = left;
            this.right = right;
        }
        
        public override Type YieldedType => typeof(T);

        public override T EvaluateTyped(ExpressionContext context) {
            return condition.EvaluateTyped(context) ? left.EvaluateTyped(context) : right.EvaluateTyped(context);
        }

        public override object Evaluate(ExpressionContext context) {
            return condition.EvaluateTyped(context) ? left.Evaluate(context) : right.Evaluate(context);
        }

    }

}