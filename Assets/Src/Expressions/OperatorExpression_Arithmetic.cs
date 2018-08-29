using System;

namespace Src {

    public static class OperatorExpression_Arithmetic {

        public static Expression Create(OperatorType operatorType, Expression left, Expression right) {

            Expression retn = CreateOperatorExpression(operatorType, left, right);

            if (IsNumericLiteralExpression(left) && IsNumericLiteralExpression(right)) {
                return CreateNumericLiteralExpression(retn.Evaluate(null));
            }

            return retn;
        }

        private static bool IsNumericLiteralExpression(Expression expression) {
            return expression is ConstantExpression<int> || expression is ConstantExpression<float> || expression is ConstantExpression<double>;
        }

        private static Expression CreateOperatorExpression(OperatorType operatorType, Expression left, Expression right) {
            Type leftType = left.YieldedType;
            Type rightType = right.YieldedType;
            TypeCode leftCode = Type.GetTypeCode(leftType);
            TypeCode rightCode = Type.GetTypeCode(rightType);
            switch (leftCode) {

                case TypeCode.Int32:
                    Expression<int> leftInt = (Expression<int>) left;
                    switch (rightCode) {
                        case TypeCode.Int16:
                            return new OperatorExpression_Arithmetic_IntShort(operatorType, leftInt, (Expression<short>) right);

                        case TypeCode.Int32:
                            return new OperatorExpression_Arithmetic_IntInt(operatorType, leftInt, (Expression<int>) right);

                        case TypeCode.Int64:
                            return new OperatorExpression_Arithmetic_IntLong(operatorType, leftInt, (Expression<long>) right);

                        case TypeCode.Single:
                            return new OperatorExpression_Arithmetic_IntFloat(operatorType, leftInt, (Expression<float>) right);

                        case TypeCode.Double:
                            return new OperatorExpression_Arithmetic_IntDouble(operatorType, leftInt, (Expression<double>) right);

                        default: throw new Exception("Operand types are not compatible");
                    }

                case TypeCode.Single:
                    Expression<float> leftFloat = (Expression<float>) left
                        ;
                    switch (rightCode) {
                        case TypeCode.Int32:
                            return new OperatorExpression_Arithmetic_FloatInt(operatorType, leftFloat, (Expression<int>) right);

                        case TypeCode.Single:
                            return new OperatorExpression_Arithmetic_FloatFloat(operatorType, leftFloat, (Expression<float>) right);

                        default: throw new Exception("Operand types are not compatible");
                    }

                case TypeCode.Double:
                    Expression<double> leftDouble = (Expression<double>) left;

                    switch (rightCode) {
                        case TypeCode.Int32:
                            return new OperatorExpression_Arithmetic_DoubleInt(operatorType, leftDouble, (Expression<int>) right);

                        case TypeCode.Single:
                            return new OperatorExpression_Arithmetic_DoubleFloat(operatorType, leftDouble, (Expression<float>) right);

                        case TypeCode.Double:
                            return new OperatorExpression_Arithmetic_DoubleDouble(operatorType, leftDouble, (Expression<double>) right);

                        default: throw new Exception("Operand types are not compatible");

                    }

                default: throw new Exception("Operand types are not compatible");

            }

        }

        private static Expression CreateNumericLiteralExpression(object value) {
            Type type = value.GetType();
            if (type == typeof(int)) {
                return new ConstantExpression<int>((int) value);
            }
            else if (type == typeof(float)) {
                return new ConstantExpression<float>((float) value);
            }
            else if (type == typeof(double)) {
                return new ConstantExpression<double>((double) value);
            }

            throw new Exception("Failed to create numeric literal from : " + value);
        }

    }

    #region Double

    public class OperatorExpression_Arithmetic_DoubleInt : Expression<double> {

        public Expression<double> left;
        public Expression<int> right;
        public OperatorType operatorType;

        public OperatorExpression_Arithmetic_DoubleInt(OperatorType operatorType, Expression<double> left, Expression<int> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(double);

        public override double EvaluateTyped(ExpressionContext context) {
            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }

            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Arithmetic_DoubleFloat : Expression<double> {

        public Expression<double> left;
        public Expression<float> right;
        public OperatorType operatorType;

        public OperatorExpression_Arithmetic_DoubleFloat(OperatorType operatorType, Expression<double> left, Expression<float> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(double);

        public override double EvaluateTyped(ExpressionContext context) {
            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }

            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Arithmetic_DoubleDouble : Expression<double> {

        public Expression<double> left;
        public Expression<double> right;
        public OperatorType operatorType;

        public OperatorExpression_Arithmetic_DoubleDouble(OperatorType operatorType, Expression<double> left, Expression<double> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(double);

        public override double EvaluateTyped(ExpressionContext context) {
            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }

            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    #endregion

    #region Float 

    public class OperatorExpression_Arithmetic_FloatInt : Expression<float> {

        public Expression<float> left;
        public Expression<int> right;
        public OperatorType operatorType;

        public OperatorExpression_Arithmetic_FloatInt(OperatorType operatorType, Expression<float> left, Expression<int> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(float);

        public override float EvaluateTyped(ExpressionContext context) {

            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }

            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Arithmetic_FloatFloat : Expression<float> {

        public Expression<float> left;
        public Expression<float> right;
        public OperatorType operatorType;

        public OperatorExpression_Arithmetic_FloatFloat(OperatorType operatorType, Expression<float> left, Expression<float> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(float);

        public override float EvaluateTyped(ExpressionContext context) {

            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }

            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    #endregion

    #region Int

    public class OperatorExpression_Arithmetic_IntShort : Expression<int> {

        public Expression<int> left;
        public Expression<short> right;
        public OperatorType operatorType;

        public OperatorExpression_Arithmetic_IntShort(OperatorType operatorType, Expression<int> left, Expression<short> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(int);

        public override int EvaluateTyped(ExpressionContext context) {
            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }

            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Arithmetic_IntInt : Expression<int> {

        public readonly Expression<int> left;
        public readonly Expression<int> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_Arithmetic_IntInt(OperatorType operatorType, Expression<int> left, Expression<int> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(int);

        public override int EvaluateTyped(ExpressionContext context) {
            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }
            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Arithmetic_IntLong : Expression<long> {

        public Expression<int> left;
        public Expression<long> right;
        public OperatorType operatorType;

        public OperatorExpression_Arithmetic_IntLong(OperatorType operatorType, Expression<int> left, Expression<long> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(long);

        public override long EvaluateTyped(ExpressionContext context) {

            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);

            }

            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Arithmetic_IntFloat : Expression<float> {

        public Expression<int> left;
        public Expression<float> right;
        public OperatorType operatorType;

        public OperatorExpression_Arithmetic_IntFloat(OperatorType operatorType, Expression<int> left, Expression<float> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(float);

        public override float EvaluateTyped(ExpressionContext context) {
            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }
            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Arithmetic_IntDouble : Expression<double> {

        public readonly Expression<int> left;
        public readonly Expression<double> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_Arithmetic_IntDouble(OperatorType operatorType, Expression<int> left, Expression<double> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(double);

        public override double EvaluateTyped(ExpressionContext context) {
            switch (operatorType) {

                case OperatorType.Plus:
                    return left.EvaluateTyped(context) + right.EvaluateTyped(context);

                case OperatorType.Minus:
                    return left.EvaluateTyped(context) - right.EvaluateTyped(context);

                case OperatorType.Times:
                    return left.EvaluateTyped(context) * right.EvaluateTyped(context);

                case OperatorType.Divide:
                    return left.EvaluateTyped(context) / right.EvaluateTyped(context);

                case OperatorType.Mod:
                    return left.EvaluateTyped(context) % right.EvaluateTyped(context);
            }
            return 0;
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    #endregion

}