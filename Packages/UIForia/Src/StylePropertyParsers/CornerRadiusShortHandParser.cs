using UIForia.Util;

namespace UIForia.Style {
    internal class CornerRadiusShortHandParser : FourPartShortHandParser<UIFixedLength> {
        public CornerRadiusShortHandParser(string shortHandName) : base(shortHandName, PropertyId.CornerRadiusTopLeft, PropertyId.CornerRadiusTopRight, PropertyId.CornerRadiusBottomRight, PropertyId.CornerRadiusBottomLeft) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIFixedLength length) {
            return parenStream.TryParseFixedLength(out length, true);
        }
    }
}