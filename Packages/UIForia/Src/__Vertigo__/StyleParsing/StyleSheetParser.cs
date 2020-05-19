using UIForia.Parsing;

namespace UIForia {

    public enum StyleNodeType {

        Style,
        Mixin,
        Animation,
        Selector,
        StyleState,
        Package,
        ConstVariable,
        Property,
        ShorthandProperty,
        RunBlock

    }

    // public struct StyleASTNode {
    //
    //     public int index;
    //     public StyleNodeType nodeType;
    //     public LineInfo lineInfo;
    //
    // }
    //
    public unsafe class StyleSheetParser3 {

        public ParseResult TryParseFile(StyleFile styleFile, Diagnostics diagnostics) {

            
            
        }

    }

}