using System.Diagnostics;

namespace Src {

    [DebuggerDisplay("{" + nameof(identifier) + "}")]
    public class IdentifierNode : ASTNode {

        public readonly string identifier;
    
        public IdentifierNode(string identifier) {
            this.identifier = identifier;
        }

    }

}