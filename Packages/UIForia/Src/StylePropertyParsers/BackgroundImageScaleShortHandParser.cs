using UIForia.Util;
using Unity.Mathematics;

namespace UIForia.Style {
    internal class BackgroundImageScaleShortHandParser : TwoPartShortHandParser<half> {
        public BackgroundImageScaleShortHandParser(string shortHandName) : base(shortHandName, PropertyId.BackgroundImageScaleX, PropertyId.BackgroundImageScaleY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out half length) {
            return parenStream.TryParseHalf(out length, true);
        }
    }
}