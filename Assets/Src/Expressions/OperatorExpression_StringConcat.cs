using System;

namespace Src {

    
    public class OperatorExpression_StringConcat<U, V> : Expression<string> {

        public readonly Expression<U> left;
        public readonly Expression<V> right;

        public OperatorExpression_StringConcat(Expression<U> left, Expression<V> right) {
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(string);
        
        public override string EvaluateTyped(ExpressionContext context) {
            return left.EvaluateTyped(context).ToString() + right.EvaluateTyped(context).ToString();
        }
        
        public override object Evaluate(ExpressionContext context) {
            return left.EvaluateTyped(context).ToString() + right.EvaluateTyped(context).ToString();
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }
        
    }
    
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

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
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

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
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

        public override bool IsConstant() {
            return right.IsConstant();
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
        public override bool IsConstant() {
            return left.IsConstant();
        }
    }

}