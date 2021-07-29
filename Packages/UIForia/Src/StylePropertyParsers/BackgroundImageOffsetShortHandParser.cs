using UIForia.Util;

namespace UIForia.Style {
    internal class BackgroundImageOffsetShortHandParser : TwoPartShortHandParser<UIFixedLength> {
        public BackgroundImageOffsetShortHandParser(string shortHandName) : base(shortHandName, PropertyId.BackgroundImageOffsetX, PropertyId.BackgroundImageOffsetY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIFixedLength length) {
            return parenStream.TryParseFixedLength(out length, true);
        }
    }
}