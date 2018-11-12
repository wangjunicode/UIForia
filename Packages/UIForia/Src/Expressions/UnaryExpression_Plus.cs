using System;

namespace UIForia {

    // Note: the Unary + operator in C# is basically a no-op. It does turn a negative number positive
    public static class UnaryExpression_PlusFactory {

        public static Expression Create(Expression expression) {
            Type yieldedType = expression.YieldedType;
            if (yieldedType == typeof(int)) {
                return new UnaryExpression_Plus_Int((Expression<int>) expression);
            }
            else if (yieldedType == typeof(float)) {
                return new UnaryExpression_Plus_Float((Expression<float>) expression);
            }
            else if (yieldedType == typeof(double)) {
                return new UnaryExpression_Plus_Double((Expression<double>) expression);
            }
            throw new Exception("Bad unary plus expression type: " + expression.YieldedType);
        }

    }

    public class UnaryExpression_Plus_Int : Expression<int> {

        public readonly Expression<int> expression;

        public UnaryExpression_Plus_Int(Expression<int> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(int);

        public override int EvaluateTyped(ExpressionContext context) {
            return +(expression.EvaluateTyped(context));
        }

        public override object Evaluate(ExpressionContext context) {
            return +(expression.EvaluateTyped(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class UnaryExpression_Plus_Float : Expression<float> {

        public readonly Expression<float> expression;

        public UnaryExpression_Plus_Float(Expression<float> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(float);

        public override float EvaluateTyped(ExpressionContext context) {
            return +(expression.EvaluateTyped(context));
        }

        public override object Evaluate(ExpressionContext context) {
            return +(expression.EvaluateTyped(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class UnaryExpression_Plus_Double : Expression<double> {

        public readonly Expression<double> expression;

        public UnaryExpression_Plus_Double(Expression<double> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(double);

        public override double EvaluateTyped(ExpressionContext context) {
            return +(expression.EvaluateTyped(context));
        }

        public override object Evaluate(ExpressionContext context) {
            return +(expression.EvaluateTyped(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}