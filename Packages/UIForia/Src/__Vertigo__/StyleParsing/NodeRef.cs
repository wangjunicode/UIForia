using UIForia.Util;

namespace UIForia.NewStyleParsing {

    public struct NodeRef {

        private int index;
        private StyleFileBuilder builder;

        public NodeRef(int index, StyleFileBuilder builder) {
            this.index = index;
            this.builder = builder;
        }

        public NodeRef AddStyleNode(in CharSpan styleName) {
            return builder.AddStyleNode(index, styleName);
        }

        public void AddPropertyNode(in PropertyParseInfo info) {
            builder.AddPropertyNode(index, info);
        }

        public void AddPropertyShorthandNode(in CharSpan propertyName, in CharSpan valueSpan) {
            builder.AddPropertyShorthandNode(index, propertyName, valueSpan);
        }

        public NodeRef AddStyleStateNode(StyleState2 state) {
            return builder.AddStyleStateNode(index, state);
        }

    }

}