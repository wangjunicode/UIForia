namespace Src {

    public class OperatorNode : ASTNode {

        public readonly OperatorType op;
        public readonly int precedence;

        public OperatorNode(int precedence, OperatorType op) {
            this.precedence = precedence;
            this.op = op;
        }

    }

}