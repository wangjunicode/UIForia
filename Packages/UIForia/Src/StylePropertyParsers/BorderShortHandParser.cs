using UIForia.Util;

namespace UIForia.Style {
    internal class BorderShortHandParser : FourPartShortHandParser<UIFixedLength> {

        public BorderShortHandParser(string shortHandName) : base(shortHandName, PropertyId.BorderTop, PropertyId.BorderBottom, PropertyId.BorderLeft, PropertyId.BorderRight) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIFixedLength length) {
            return parenStream.TryParseFixedLength(out length, true);
        }
    }
}