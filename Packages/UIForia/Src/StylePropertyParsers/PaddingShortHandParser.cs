using UIForia.Util;

namespace UIForia.Style {
    internal class PaddingShortHandParser : FourPartShortHandParser<UISpaceSize> {
        public PaddingShortHandParser(string shortHandName) : base(shortHandName, PropertyId.PaddingTop, PropertyId.PaddingBottom, PropertyId.PaddingLeft, PropertyId.PaddingRight) { }
        
        protected override bool TryParseValue(ref CharStream parenStream, out UISpaceSize length) {
            return parenStream.TryParseSpaceSize(out length, true);
        }
    }
}