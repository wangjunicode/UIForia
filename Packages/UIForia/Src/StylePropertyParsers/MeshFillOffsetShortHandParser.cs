using UIForia.Util;

namespace UIForia.Style {
    internal class MeshFillOffsetShortHandParser : TwoPartShortHandParser<UIFixedLength> {
        public MeshFillOffsetShortHandParser(string shortHandName) : base(shortHandName, PropertyId.MeshFillOffsetX, PropertyId.MeshFillOffsetY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIFixedLength length) {
            return parenStream.TryParseFixedLength(out length, true);
        }
    }
}