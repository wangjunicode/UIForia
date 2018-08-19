using System;

namespace Src {

    public static class ParenExpressionFactory {

        public static Expression CreateParenExpression(Expression expression) {
            Type type = expression.YieldedType;

            if (type == typeof(string)) {
                return new ParenExpression_Typed<string>((Expression<string>) expression);
            }
            else if (type == typeof(int)) {
                return new ParenExpression_Typed<int>((Expression<int>) expression);

            }
            else if (type == typeof(float)) {
                return new ParenExpression_Typed<float>((Expression<float>) expression);

            }
            else if (type == typeof(double)) {
                return new ParenExpression_Typed<double>((Expression<double>) expression);

            }
            else if (type == typeof(bool)) {
                return new ParenExpression_Typed<bool>((Expression<bool>) expression);

            }
            else {
                return new ParenExpression_Generic(expression);
            }
        }

    }

    public class ParenExpression_Generic : Expression {

        public readonly Expression expression;

        public ParenExpression_Generic(Expression expression) {
            this.expression = expression;
        }

        public override Type YieldedType => expression.YieldedType;

        public override object Evaluate(ExpressionContext context) {
            return expression.Evaluate(context);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class ParenExpression_Typed<T> : Expression<T> {

        public readonly Expression<T> expression;

        public ParenExpression_Typed(Expression<T> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(T);

        public override T EvaluateTyped(ExpressionContext context) {
            return expression.EvaluateTyped(context);
        }

        public override object Evaluate(ExpressionContext context) {
            return expression.EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}