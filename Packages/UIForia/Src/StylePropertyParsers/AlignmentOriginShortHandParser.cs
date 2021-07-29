using UIForia.Util;

namespace UIForia.Style {
    internal class AlignmentOriginShortHandParser : TwoPartShortHandParser<UIOffset> {
        public AlignmentOriginShortHandParser(string shortHandName) : base(shortHandName, PropertyId.AlignmentOriginX, PropertyId.AlignmentOriginY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIOffset value) {
            return parenStream.TryParseOffsetMeasurement(out value, true);
        }
    }
}