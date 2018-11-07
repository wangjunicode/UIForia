namespace UIForia {

    public interface IOperatorNode {

        int Precedence { get; }
        OperatorType OpType { get; }

    }

    public class OperatorNode : ASTNode, IOperatorNode {

        public readonly OperatorType op;
        public readonly int precedence;

        public OperatorNode(int precedence, OperatorType op) {
            this.precedence = precedence;
            this.op = op;
        }

        public int Precedence => precedence;
        public OperatorType OpType => op;

    }

}