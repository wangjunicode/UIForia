using UIForia.Util;

namespace UIForia.Style2 {

    public struct ParsedMixin {

        public readonly CharSpan name;
        public readonly uint rangeStart;
        public readonly uint rangeEnd;

        // todo - this is basically the same as ParsedStyle, combine?
        public ParsedMixin(in CharSpan name, uint rangeStart, uint rangeEnd) {
            this.name = name;
            this.rangeStart = rangeStart;
            this.rangeEnd = rangeEnd;
        }

    }

}