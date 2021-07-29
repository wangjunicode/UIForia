using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {
    internal class BorderColorShortHandParser : FourPartShortHandParser<UIColor> {
        public BorderColorShortHandParser(string shortHandName) : base(shortHandName, PropertyId.BorderColorTop, PropertyId.BorderColorBottom, PropertyId.BorderColorLeft, PropertyId.BorderColorRight) { }

        protected override bool TryParseValue(ref CharStream parenStream, out UIColor value) {
            if (parenStream.TryParseColorProperty(out Color32 color)) {
                value = color;
                return true;
            }

            value = default;
            return false;
        }
    }
}