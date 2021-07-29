using UIForia.Util;

namespace UIForia.Style {
    internal class AlignmentBoundaryShortHandParser : TwoPartShortHandParser<AlignmentBoundary> {
        public AlignmentBoundaryShortHandParser(string shortHandName) : base(shortHandName, PropertyId.AlignmentBoundaryX, PropertyId.AlignmentBoundaryY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out AlignmentBoundary value) {
            bool result = parenStream.TryParseEnum<AlignmentBoundary>(out int enumValue);
            value = (AlignmentBoundary) enumValue;
            return result;
        }
    }
}