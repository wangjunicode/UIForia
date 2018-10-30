using System;

namespace Src {

    public class OperatorExpression_Equality<U, V> : Expression<bool> {

        public readonly Expression<U> left;
        public readonly Expression<V> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_Equality(OperatorType operatorType, Expression<U> left, Expression<V> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            if (operatorType == OperatorType.Equals) {
                return left.EvaluateTyped(context).Equals(right.EvaluateTyped(context));
            }
            else if (operatorType == OperatorType.NotEquals) {
                return !(left.EvaluateTyped(context).Equals(right.EvaluateTyped(context)));
            }
            throw new Exception("Invalid equality operator: " + operatorType);
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }
    
    public class OperatorExpression_AndOrObject<U, V> : Expression<bool> where U : class where V : class {

        public readonly Expression<U> left;
        public readonly Expression<V> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_AndOrObject(OperatorType operatorType, Expression<U> left, Expression<V> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.EvaluateTyped(context) != null && right.EvaluateTyped(context) != null;
            }
            else if (operatorType == OperatorType.Or) {
                return left.EvaluateTyped(context) != null || right.EvaluateTyped(context) != null;
            }
            throw new Exception("Invalid and or operator: " + operatorType);
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }
    
    public class OperatorExpression_AndOrBool : Expression<bool> {

        public readonly Expression<bool> left;
        public readonly Expression<bool> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_AndOrBool(OperatorType operatorType, Expression<bool> left, Expression<bool> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.EvaluateTyped(context) && right.EvaluateTyped(context);
            }
            else if (operatorType == OperatorType.Or) {
                return left.EvaluateTyped(context)  || right.EvaluateTyped(context) ;
            }
            throw new Exception("Invalid and or operator: " + operatorType);
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }
    
    public class OperatorExpression_AndOrObjectBool<U> : Expression<bool> where U : class {

        public readonly Expression<U> left;
        public readonly Expression<bool> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_AndOrObjectBool(OperatorType operatorType, Expression<U> left, Expression<bool> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.EvaluateTyped(context) != null && right.EvaluateTyped(context);
            }
            else if (operatorType == OperatorType.Or) {
                return left.EvaluateTyped(context) != null || right.EvaluateTyped(context);
            }
            throw new Exception("Invalid and or operator: " + operatorType);
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }
    
    public class OperatorExpression_AndOrBoolObject<V> : Expression<bool> where V : class {

        public readonly Expression<bool> left;
        public readonly Expression<V> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_AndOrBoolObject(OperatorType operatorType, Expression<bool> left, Expression<V> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.EvaluateTyped(context) && right.EvaluateTyped(context) != null;
            }
            else if (operatorType == OperatorType.Or) {
                return left.EvaluateTyped(context)  || right.EvaluateTyped(context) != null;
            }
            throw new Exception("Invalid and or operator: " + operatorType);
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

}