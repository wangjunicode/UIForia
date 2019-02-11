using System;
using System.Collections.Generic;

namespace UIForia.AttributeProcessors {

    public interface IAttributeProcessor {

        void Process(UIElement element, UITemplate template, ElementAttribute currentAttr, IReadOnlyList<ElementAttribute> attributes);

    }

    public class RouteAttrProcessor : IAttributeProcessor {

        public void Process(UIElement element, UITemplate template, ElementAttribute currentAttr, IReadOnlyList<ElementAttribute> attributes) {
            if (currentAttr.name != "route") return;

            if (TryGetAttribute("onRouteEnter", attributes, out ElementAttribute onRouteEnterAttr)) {
                    
            }
            
            if (TryGetAttribute("onRouteEnter", attributes, out ElementAttribute onRouteChangedAttr)) {
                
            }
            
            if (TryGetAttribute("onRouteEnter", attributes, out ElementAttribute onRouteExitAttr)) {
                
            }
            
            
            
        }

        private static bool TryGetAttribute(string name, IReadOnlyList<ElementAttribute> attributes, out ElementAttribute retn) {
            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].name == name) {
                    retn = attributes[i];
                    return true;
                }
            }

            retn = default;
            return false;
        }

    }

}