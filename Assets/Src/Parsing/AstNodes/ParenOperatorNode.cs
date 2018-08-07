namespace Src {

    public class ParenOperatorNode : OperatorNode {

        public readonly ExpressionNode expression;
        
        public ParenOperatorNode(ExpressionNode expression) : base (-1, OperatorType.Paren) {
            this.expression = expression;
        }

    }

}