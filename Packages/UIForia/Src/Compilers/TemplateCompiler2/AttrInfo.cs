using UIForia.Parsing;

namespace UIForia.Compilers {

    public readonly struct AttrInfo {

        public readonly string key;
        public readonly string value;
        public readonly string rawValue;
        public readonly int depth;
        public readonly int line;
        public readonly int column;
        public readonly AttributeType type;
        public readonly AttributeFlags flags;

        public AttrInfo(int depth, in AttributeDefinition attr) {
            this.depth = depth;
            this.key = attr.key;
            this.value = attr.value;
            this.line = attr.line;
            this.column = attr.column;
            this.type = attr.type;
            this.flags = attr.flags;
            this.rawValue = attr.rawValue;
        }

    }

}