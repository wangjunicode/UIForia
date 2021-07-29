using UIForia.Util;
using Unity.Mathematics;

namespace UIForia.Style {
    internal class BackgroundImageTileShortHandParser : TwoPartShortHandParser<half> {
        public BackgroundImageTileShortHandParser(string shortHandName) : base(shortHandName, PropertyId.BackgroundImageTileX, PropertyId.BackgroundImageTileY) { }

        protected override bool TryParseValue(ref CharStream parenStream, out half value) {
            return parenStream.TryParseHalf(out value, true);
        }
    }
}