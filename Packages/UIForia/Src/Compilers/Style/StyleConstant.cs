using System;
using UIForia.Parsing.Style.AstNodes;

namespace UIForia.Compilers.Style {
    
    public struct StyleConstant {

        public string name;

        public Type type;

        public StyleASTNode value;

        public ReferenceNode referenceNode;

        public bool exported;
    }
}
