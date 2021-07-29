using UIForia.Util;

namespace UIForia.Style {
    internal class SpaceBetweenShortHandParser : TwoPartShortHandParser<UISpaceSize> {
        public SpaceBetweenShortHandParser(string shortHandName) : base(shortHandName, PropertyId.SpaceBetweenHorizontal, PropertyId.SpaceBetweenVertical) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UISpaceSize value) {
            return parenStream.TryParseSpaceSize(out value, true);
        }
    }
}