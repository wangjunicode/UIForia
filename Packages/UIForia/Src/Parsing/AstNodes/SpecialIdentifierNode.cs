using System;
using System.Diagnostics;

namespace UIForia {

    [DebuggerDisplay("{" + nameof(identifier) + "}")]
    public class SpecialIdentifierNodeOld : IdentifierNodeOld {

        public SpecialIdentifierNodeOld(string identifier) : base(identifier) {}

     
    }

}