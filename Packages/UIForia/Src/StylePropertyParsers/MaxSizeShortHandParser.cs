using UIForia.Util;

namespace UIForia.Style {
    internal class MaxSizeShortHandParser : TwoPartShortHandParser<UISizeConstraint> {
        public MaxSizeShortHandParser(string shortHandName) : base(shortHandName, PropertyId.MaxWidth, PropertyId.MaxHeight) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UISizeConstraint value) {
            return parenStream.TryParseSizeConstraint(out value, true);
        }
    }
}