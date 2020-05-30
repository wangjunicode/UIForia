using UIForia.Style;
using UIForia.Util;

namespace UIForia.NewStyleParsing {

    public struct PropertyParseInfo {

        public PropertyId propertyId;
        public CharSpan identifierSpan;
        public CharSpan valueSpan;
        public string propertyName;

        public bool isShorthand {
            get => propertyId < 0;
        }

    }

}