namespace Src {

    public class ArrayAccessExpressionPart : AccessExpressionPart {

        public readonly ExpressionNode expression;

        public ArrayAccessExpressionPart(ExpressionNode expression) {
            this.expression = expression;
        }

    }

}