using UIForia.Util;

namespace UIForia.Style {

    public class FloatParser : IStylePropertyParser {

        public bool TryParse(CharStream stream, PropertyId propertyId, out StyleProperty2 property) {
            if (stream.TryParseFloat(out float value)) {
                property = new StyleProperty2(propertyId, value);
                return true;
            }

            property = default;
            return false;
        }

    }

}