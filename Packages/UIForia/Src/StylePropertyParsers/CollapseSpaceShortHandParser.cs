using UIForia.Util;

namespace UIForia.Style {
    internal class CollapseSpaceShortHandParser : TwoPartShortHandParser<SpaceCollapse> {
        public CollapseSpaceShortHandParser(string shortHandName) : base(shortHandName, PropertyId.CollapseSpaceHorizontal, PropertyId.CollapseSpaceVertical) { }

        protected override bool TryParseValue(ref CharStream parenStream, out SpaceCollapse value) {
            bool result = parenStream.TryParseEnum<SpaceCollapse>(out int enumValue);
            value = (SpaceCollapse) enumValue;
            return result;
        }
    }
}