using Rendering;

namespace Src.Systems {

    public class StyleSystem : ISystem {

        private readonly UIView view;
        
        public StyleSystem(UIView view) {
            this.view = view;
        }
        
        public void OnReset() {
            
        }

        public void OnUpdate() {
            
        }

        public void OnDestroy() {
            
        }

        public void OnElementCreated(UIElementCreationData elementData) {
            UIElement element = elementData.element;
            StyleDefinition style = elementData.style;
            UITemplateContext context = elementData.context;
            // todo -- remove view from element.style, replace with 'this'
            element.style = new UIStyleSet(element, view);
            
            if (style.constantBindings != null) {
                for (var i = 0; i < style.constantBindings.Length; i++) {
                    style.constantBindings[i].Apply(element.style, context);
                }
            }

            // todo -- maybe this is where external style paths get resolved
            if (style.baseStyles != null) {
                for (int i = 0; i < style.baseStyles.Count; i++) {
                    element.style.AddBaseStyle(style.baseStyles[i]);
                }
            }
            
        }

        public void OnElementEnabled(UIElement element) {
            
        }

        public void OnElementDisabled(UIElement element) {
            
        }

        public void OnElementDestroyed(UIElement element) {
            
        }

    }

}