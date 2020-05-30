using UIForia.Util;

namespace UIForia.Style {

    public interface IStylePropertyParser {

        bool TryParse(CharStream stream, PropertyId propertyId, Diagnostics diagnostics, out StyleProperty2 property);
        
        bool TryParseFromBinding(CharStream stream, PropertyId propertyId, Diagnostics diagnostics,  out StyleProperty2 property);

    }
    
    public interface IStyleShorthandParser {
        
        // shorthand parser needs to return 'default' properties when not setting them in order to remove previously set ones
        bool TryParse(CharStream stream, Diagnostics diagnostics, StructList<StyleProperty2> output);
        
        bool TryParseFromBinding(CharStream stream, Diagnostics diagnostics, StructList<StyleProperty2> output);

    }

}