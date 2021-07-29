using UIForia.Util;

namespace UIForia.Style {
    internal class MarginShortHandParser : FourPartShortHandParser<UISpaceSize> {
        public MarginShortHandParser(string shortHandName) : base(shortHandName, PropertyId.MarginTop, PropertyId.MarginBottom, PropertyId.MarginLeft, PropertyId.MarginRight) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UISpaceSize length) {
            return parenStream.TryParseSpaceSize(out length, true);
        }
    }
}