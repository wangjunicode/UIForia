using UIForia.Util;

namespace UIForia.Style {
    internal class CornerBevelShortHandParser : FourPartShortHandParser<UIFixedLength> {
        public CornerBevelShortHandParser(string shortHandName) : base(shortHandName, PropertyId.CornerBevelTopLeft, PropertyId.CornerBevelTopRight, PropertyId.CornerBevelBottomRight, PropertyId.CornerBevelBottomLeft) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIFixedLength length) {
            return parenStream.TryParseFixedLength(out length, true);
        }
    }
}