using UIForia.Util;

namespace UIForia.Style2 {

    internal struct ParsedStyle {

        public readonly CharSpan name;
        public readonly int rangeStart;
        public readonly int rangeEnd;

        public ParsedStyle(CharSpan name, int rangeStart, int rangeEnd) {
            this.name = name;
            this.rangeStart = rangeStart;
            this.rangeEnd = rangeEnd;
        }

    }

}