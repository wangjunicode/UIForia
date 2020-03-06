using UIForia.Util;

namespace UIForia.Style {

    public interface IStylePropertyParser {

        bool TryParse(CharStream stream, PropertyId propertyId, out StyleProperty2 property);

    }
    
    public interface IStyleShorthandParser {

        bool TryParse(CharStream stream, StructList<StyleProperty2> output);

    }

}