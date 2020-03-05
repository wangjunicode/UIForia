using UIForia.Util;

namespace UIForia.Style2 {

    internal struct ParsedStyle {

        public int partEnd;
        public readonly int partStart;
        public readonly CharSpan name;

        public ParsedStyle(CharSpan name, int partStart) {
            this.name = name;
            this.partStart = partStart;
            this.partEnd = 0;
        }

    }

}