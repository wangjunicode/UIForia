using System.Collections.Generic;
using UIForia.Util;

namespace UIForia {

    public struct TemplateScope {

        public UITemplateContext context;
        // add slots
        public readonly UIElement rootElement;
        
        public TemplateScope(UIElement rootElement, UITemplateContext context) {
            this.rootElement = rootElement;
            this.context = context;
        }
        
    }

}