namespace Src {

    public struct UIElementCreationData {

        public readonly string name;
        public readonly UIElement element;
        public readonly Binding[] bindings;
        public readonly StyleDefinition style;
        public readonly UITemplateContext context;

        public UIElementCreationData(
            string name,
            UIElement element,
            StyleDefinition style,
            Binding[] bindings,
            UITemplateContext context) {

            this.name = name;
            this.element = element;
            this.style = style;
            this.bindings = bindings;
            this.context = context;

        }

        public int GetDepth() {
            int depth = 0;
            UIElement ptr = element;
            while (ptr != null) {
                ptr = ptr.parent;
                depth++;
            }
            return depth;
        }

    }

}