namespace Src {

    public struct UIElementCreationData {

        public string name;
        public UIElement element;
        public Binding[] bindings;
        public StyleDefinition style;
        public UITemplateContext context;

        public UIElementCreationData(string name, UIElement element, UITemplateContext context, StyleDefinition style = null, Binding[] bindings = null) {
            this.name = name;
            this.element = element;
            this.context = context;
            this.style = style;
            this.bindings = bindings ?? Binding.EmptyArray;
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