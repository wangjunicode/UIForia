using UIForia.Parsing;

namespace UIForia.Compilers {

    internal class CompiledStyleBlock {

        public int propertyStart;
        public ParseBlockId parseBlockId;

        public CompiledStyleBlock firstChild;
        public CompiledStyleBlock nextSibling;

        public int transitionStart;
        // public int transitionCount;

        public int animationActionStart;
        public int animationActionCount;
    }

}