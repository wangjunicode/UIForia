using UIForia.Util;

namespace UIForia.Style {
    internal class PreferredSizeShortHandParser : TwoPartShortHandParser<UIMeasurement> {
        public PreferredSizeShortHandParser(string shortHandName) : base(shortHandName, PropertyId.PreferredWidth, PropertyId.PreferredHeight) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIMeasurement value) {
            return parenStream.TryParseMeasurement(out value, true);
        }
    }
}