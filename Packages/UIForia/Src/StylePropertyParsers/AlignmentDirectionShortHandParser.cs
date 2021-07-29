using UIForia.Util;

namespace UIForia.Style {
    internal class AlignmentDirectionShortHandParser : TwoPartShortHandParser<AlignmentDirection> {
        public AlignmentDirectionShortHandParser(string shortHandName) : base(shortHandName, PropertyId.AlignmentDirectionX, PropertyId.AlignmentDirectionY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out AlignmentDirection value) {
            bool result = parenStream.TryParseEnum<AlignmentDirection>(out int enumValue);
            value = (AlignmentDirection) enumValue;
            return result;
        }
    }
}