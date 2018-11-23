namespace UIForia {

    public struct TemplateScope {

        // add slots
        public UITemplateContext context;
        public readonly UIElement rootElement;
        
        public TemplateScope(UIElement rootElement, UITemplateContext context) {
            this.rootElement = rootElement;
            this.context = context;
        }
        
    }

}