using UIForia.Util;

namespace UIForia.Style {
    internal class OverflowShortHandParser : TwoPartShortHandParser<Overflow> {
        public OverflowShortHandParser(string shortHandName) : base(shortHandName, PropertyId.OverflowX, PropertyId.OverflowY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out Overflow value) {
            bool result = parenStream.TryParseEnum<Overflow>(out int enumValue);
            value = (Overflow) enumValue;
            return result;
        }
    }
}