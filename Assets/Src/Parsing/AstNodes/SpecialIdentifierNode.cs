using System;
using System.Diagnostics;

namespace UIForia {

    [DebuggerDisplay("{" + nameof(identifier) + "}")]
    public class SpecialIdentifierNode : IdentifierNode {

        public SpecialIdentifierNode(string identifier) : base(identifier) {}

     
    }

}