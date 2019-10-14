using System;
using UIForia.Parsing.Expressions.AstNodes;

namespace UIForia.Expressions {

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

        public override bool Evaluate(ExpressionContext context) {
            
            // these mostly run via class types so no boxing, but if a struct type does not
            // define an equality operator for the target type then this will be used, which will box 
            // not sure how to avoid it
            
            if (operatorType == OperatorType.Equals) {
                return Equals(left.Evaluate(context), right.Evaluate(context));
            }
            else if (operatorType == OperatorType.NotEquals) {
                return !Equals(left.Evaluate(context), right.Evaluate(context));
            }
            
            throw new Exception("Invalid equality operator: " + operatorType);
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

        public override bool Evaluate(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.Evaluate(context) != null && right.Evaluate(context) != null;
            }
            else if (operatorType == OperatorType.Or) {
                return left.Evaluate(context) != null || right.Evaluate(context) != null;
            }
            throw new Exception("Invalid and or operator: " + operatorType);
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

        public override bool Evaluate(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.Evaluate(context) && right.Evaluate(context);
            }
            else if (operatorType == OperatorType.Or) {
                return left.Evaluate(context)  || right.Evaluate(context) ;
            }
            throw new Exception("Invalid and or operator: " + operatorType);
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

        public override bool Evaluate(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.Evaluate(context) != null && right.Evaluate(context);
            }
            else if (operatorType == OperatorType.Or) {
                return left.Evaluate(context) != null || right.Evaluate(context);
            }
            throw new Exception("Invalid and or operator: " + operatorType);
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

        public override bool Evaluate(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.Evaluate(context) && right.Evaluate(context) != null;
            }
            else if (operatorType == OperatorType.Or) {
                return left.Evaluate(context)  || right.Evaluate(context) != null;
            }
            throw new Exception("Invalid and or operator: " + operatorType);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

}