using UIForia.Util;

namespace UIForia.Style {
    internal class MinSizeShortHandParser : TwoPartShortHandParser<UISizeConstraint> {
        public MinSizeShortHandParser(string shortHandName) : base(shortHandName, PropertyId.MinWidth, PropertyId.MinHeight) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UISizeConstraint value) {
            return parenStream.TryParseSizeConstraint(out value, true);
        }
    }
}