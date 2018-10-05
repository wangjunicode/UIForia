using System;
using System.Diagnostics;

namespace Src {

    [DebuggerDisplay("{" + nameof(identifier) + "}")]
    public class SpecialIdentifierNode : IdentifierNode {

        public SpecialIdentifierNode(string identifier) : base(identifier) {}

     
    }

}