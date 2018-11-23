namespace UIForia {

    public class UITemplateContext : ExpressionContext {

        public UITemplateContext(object rootObject) : base(rootObject) { }
        
        public UIElement rootElement => (UIElement) rootObject;
        public UIElement currentElement => (UIElement) currentObject;

    }

}