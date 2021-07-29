using UIForia.Util;

namespace UIForia.Style {
    internal class AlignmentTargetShortHandParser : TwoPartShortHandParser<AlignmentTarget> {
        public AlignmentTargetShortHandParser(string shortHandName) : base(shortHandName, PropertyId.AlignmentTargetX, PropertyId.AlignmentTargetY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out AlignmentTarget value) {
            bool result = parenStream.TryParseEnum<AlignmentTarget>(out int enumValue);
            value = (AlignmentTarget) enumValue;
            return result;
        }
    }
}