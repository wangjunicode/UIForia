using System;

namespace Src {

    public class OperatorExpression_StringConcat : Expression<string> {

        public readonly Expression left;
        public readonly Expression right;

        public OperatorExpression_StringConcat(Expression left, Expression right) {
            this.left = left;
            this.right = right;
        }

        public override string EvaluateTyped(ExpressionContext context) {
            return left.Evaluate(context) + right.Evaluate(context).ToString();
        }

        public override Type YieldedType => typeof(string);

        public override object Evaluate(ExpressionContext context) {
            return left.Evaluate(context) + right.Evaluate(context).ToString();
        }

        public static Expression Create(Expression left, Expression right) {
            if (left.YieldedType == typeof(string) && right.YieldedType == typeof(string)) {
                return new OperatorExpression_StringConcat_Typed((Expression<string>) left, (Expression<string>) right);
            }
            return new OperatorExpression_StringConcat(left, right);
        }

    }

    public class OperatorExpression_StringConcat_Typed : Expression<string> {

        public readonly Expression<string> left;
        public readonly Expression<string> right;

        public OperatorExpression_StringConcat_Typed(Expression<string> left, Expression<string> right) {
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(string);

        public override object Evaluate(ExpressionContext context) {
            return left.EvaluateTyped(context) + right.EvaluateTyped(context);
        }

        public override string EvaluateTyped(ExpressionContext context) {
            return left.EvaluateTyped(context) + right.EvaluateTyped(context);
        }

    }

    public class OperatorExpression_StringConcat_Const_Dynamic : Expression<string> {

        public readonly string left;
        public readonly Expression<string> right;

        public override Type YieldedType => typeof(string);

        public OperatorExpression_StringConcat_Const_Dynamic(string left, Expression<string> right) {
            this.left = left;
            this.right = right;
        }

        public override object Evaluate(ExpressionContext context) {
            return left + right.EvaluateTyped(context);
        }

        public override string EvaluateTyped(ExpressionContext context) {
            return left + right.EvaluateTyped(context);
        }

    }

    public class OperatorExpression_StringConcat_Dynamic_Const : Expression<string> {

        public readonly Expression<string> left;
        public readonly string right;

        public override Type YieldedType => typeof(string);

        public OperatorExpression_StringConcat_Dynamic_Const(Expression<string> left, string right) {
            this.left = left;
            this.right = right;
        }

        public override object Evaluate(ExpressionContext context) {
            return left.EvaluateTyped(context) + right;
        }

        public override string EvaluateTyped(ExpressionContext context) {
            return left.EvaluateTyped(context) + right;
        }

    }

}