namespace Src {

    public struct RegistrationData {

        public readonly UIElement element;
        public readonly Binding[] bindings;
        public readonly UITemplateContext context;

        public int depth;
        
        public RegistrationData(UIElement element, Binding[] bindings, UITemplateContext context) {
            this.element = element;
            this.bindings = bindings;
            this.context = context;
            this.depth = 0;
        }

    }

}