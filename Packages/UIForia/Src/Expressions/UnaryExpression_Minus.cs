using System;

namespace UIForia {

    public static class UnaryExpression_MinusFactory {

        public static Expression Create(Expression expression) {
            Type yieldedType = expression.YieldedType;
            if (yieldedType == typeof(int)) {
                return new UnaryExpression_Minus_Int((Expression<int>) expression);
            }
            else if (yieldedType == typeof(float)) {
                return new UnaryExpression_Minus_Float((Expression<float>) expression);
            }
            else if (yieldedType == typeof(double)) {
                return new UnaryExpression_Minus_Double((Expression<double>) expression);
            }
            throw new Exception("Bad unary minus expression type: " + expression.YieldedType);
        }

    }

    public class UnaryExpression_Minus_Int : Expression<int> {

        public readonly Expression<int> expression;

        public UnaryExpression_Minus_Int(Expression<int> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(int);

        public override int Evaluate(ExpressionContext context) {
            return -(expression.Evaluate(context));
        }       

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class UnaryExpression_Minus_Float : Expression<float> {

        public readonly Expression<float> expression;

        public UnaryExpression_Minus_Float(Expression<float> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(float);

        public override float Evaluate(ExpressionContext context) {
            return -(expression.Evaluate(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class UnaryExpression_Minus_Double : Expression<double> {

        public readonly Expression<double> expression;

        public UnaryExpression_Minus_Double(Expression<double> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(double);

        public override double Evaluate(ExpressionContext context) {
            return -(expression.Evaluate(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}