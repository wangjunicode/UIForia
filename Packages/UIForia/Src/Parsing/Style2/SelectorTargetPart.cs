using UIForia.Util;

namespace UIForia.Style2 {

    public struct SelectorTargetPart {

        public SelectorTargetPartType type;
        public CharSpan span0;
        public CharSpan span1;

    }
    
    public class ParsedSelector {

        public LightList<SelectorTargetPart> targetParts;

    }

    public enum SelectorTargetPartType {

        StyleName,
        Tag,
        State,
        Attribute_Present,
        Attribute_StartsWith,
        Attribute_EndsWith,
        Attribute_Contains,
        Modifier_FirstChild,
        Modifier_LastChild,
        Modifier_NthChild,
        Modifier_OnlyChild,
            
    }

}