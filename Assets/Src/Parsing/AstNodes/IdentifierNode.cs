namespace Src {

    public class IdentifierNode : ASTNode {

        public readonly string identifier;

        public IdentifierNode(string identifier) {
            this.identifier = identifier;
        }

    }

}