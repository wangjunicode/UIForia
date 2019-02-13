using System;
using UIForia.Parsing;

namespace UIForia {

    public static class OperatorExpression_Comparison {

        public static Expression Create(OperatorType operatorType, Expression left, Expression right) {
            return CreateOperatorExpression(operatorType, left, right);
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
                            return new OperatorExpression_Comparison_IntShort(operatorType, leftInt, (Expression<short>) right);

                        case TypeCode.Int32:
                            return new OperatorExpression_Comparison_IntInt(operatorType, leftInt, (Expression<int>) right);

                        case TypeCode.Int64:
                            return new OperatorExpression_Comparison_IntLong(operatorType, leftInt, (Expression<long>) right);

                        case TypeCode.Single:
                            return new OperatorExpression_Comparison_IntFloat(operatorType, leftInt, (Expression<float>) right);

                        case TypeCode.Double:
                            return new OperatorExpression_Comparison_IntDouble(operatorType, leftInt, (Expression<double>) right);

                        default: throw new Exception("Operand types are not compatible");
                    }

                case TypeCode.Single:
                    Expression<float> leftFloat = (Expression<float>) left
                        ;
                    switch (rightCode) {
                        case TypeCode.Int32:
                            return new OperatorExpression_Comparison_FloatInt(operatorType, leftFloat, (Expression<int>) right);

                        case TypeCode.Single:
                            return new OperatorExpression_Comparison_FloatFloat(operatorType, leftFloat, (Expression<float>) right);

                        default: throw new Exception("Operand types are not compatible");
                    }

                case TypeCode.Double:
                    Expression<double> leftDouble = (Expression<double>) left;

                    switch (rightCode) {
                        case TypeCode.Int32:
                            return new OperatorExpression_Comparison_DoubleInt(operatorType, leftDouble, (Expression<int>) right);

                        case TypeCode.Single:
                            return new OperatorExpression_Comparison_DoubleFloat(operatorType, leftDouble, (Expression<float>) right);

                        case TypeCode.Double:
                            return new OperatorExpression_Comparison_DoubleDouble(operatorType, leftDouble, (Expression<double>) right);

                        default: throw new Exception("Operand types are not compatible");
                    }

                default: throw new Exception("Operand types are not compatible");
            }
        }

    }

    #region Double

    public class OperatorExpression_Comparison_DoubleInt : Expression<bool> {

        public Expression<double> left;
        public Expression<int> right;
        public OperatorType operatorType;

        public OperatorExpression_Comparison_DoubleInt(OperatorType operatorType, Expression<double> left, Expression<int> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Comparison_DoubleFloat : Expression<bool> {

        public Expression<double> left;
        public Expression<float> right;
        public OperatorType operatorType;

        public OperatorExpression_Comparison_DoubleFloat(OperatorType operatorType, Expression<double> left, Expression<float> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Comparison_DoubleDouble : Expression<bool> {

        public Expression<double> left;
        public Expression<double> right;
        public OperatorType operatorType;

        public OperatorExpression_Comparison_DoubleDouble(OperatorType operatorType, Expression<double> left, Expression<double> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    #endregion

    #region Float 

    public class OperatorExpression_Comparison_FloatInt : Expression<bool> {

        public Expression<float> left;
        public Expression<int> right;
        public OperatorType operatorType;

        public OperatorExpression_Comparison_FloatInt(OperatorType operatorType, Expression<float> left, Expression<int> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
                
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Comparison_FloatFloat : Expression<bool> {

        public Expression<float> left;
        public Expression<float> right;
        public OperatorType operatorType;

        public OperatorExpression_Comparison_FloatFloat(OperatorType operatorType, Expression<float> left, Expression<float> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    #endregion

    #region Int

    public class OperatorExpression_Comparison_IntShort : Expression<bool> {

        public Expression<int> left;
        public Expression<short> right;
        public OperatorType operatorType;

        public OperatorExpression_Comparison_IntShort(OperatorType operatorType, Expression<int> left, Expression<short> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Comparison_IntInt : Expression<bool> {

        public readonly Expression<int> left;
        public readonly Expression<int> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_Comparison_IntInt(OperatorType operatorType, Expression<int> left, Expression<int> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Comparison_IntLong : Expression<bool> {

        public Expression<int> left;
        public Expression<long> right;
        public OperatorType operatorType;

        public OperatorExpression_Comparison_IntLong(OperatorType operatorType, Expression<int> left, Expression<long> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Comparison_IntFloat : Expression<bool> {

        public Expression<int> left;
        public Expression<float> right;
        public OperatorType operatorType;

        public OperatorExpression_Comparison_IntFloat(OperatorType operatorType, Expression<int> left, Expression<float> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    public class OperatorExpression_Comparison_IntDouble : Expression<bool> {

        public readonly Expression<int> left;
        public readonly Expression<double> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_Comparison_IntDouble(OperatorType operatorType, Expression<int> left, Expression<double> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return left.Evaluate(context) > right.Evaluate(context);

                case OperatorType.GreaterThanEqualTo:
                    return left.Evaluate(context) >= right.Evaluate(context);

                case OperatorType.LessThan:
                    return left.Evaluate(context) < right.Evaluate(context);

                case OperatorType.LessThanEqualTo:
                    return left.Evaluate(context) <= right.Evaluate(context);
                
                case OperatorType.Equals:
                    return left.Evaluate(context) == right.Evaluate(context);
                
                case OperatorType.NotEquals:
                    return left.Evaluate(context) != right.Evaluate(context);
            }

            return false;
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

    #endregion

}