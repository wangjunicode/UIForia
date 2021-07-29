using UIForia.Util;

namespace UIForia.Style {
    internal class AlignmentOffsetShortHandParser : TwoPartShortHandParser<UIOffset> {
        public AlignmentOffsetShortHandParser(string shortHandName) : base(shortHandName, PropertyId.AlignmentOffsetX, PropertyId.AlignmentOffsetY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIOffset value) {
            return parenStream.TryParseOffsetMeasurement(out value, true);
        }
    }
}