using UIForia.Util;

namespace UIForia.Style {

    public interface IStylePropertyParser {

        bool TryParse(CharStream stream, PropertyId propertyId, out StyleProperty2 property);

    }

}