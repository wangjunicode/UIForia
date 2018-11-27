using System.Diagnostics;

namespace UIForia {

    [DebuggerDisplay("{" + nameof(identifier) + "}")]
    public class IdentifierNodeOld : ASTNode_Old {

        public readonly string identifier;
    
        public IdentifierNodeOld(string identifier) {
            this.identifier = identifier;
        }

    }

}