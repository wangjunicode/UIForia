using System;
using System.Collections.Generic;
using UIForia.Routing2;
using UnityEngine;

namespace UIForia.AttributeProcessors {

    public interface IAttributeProcessor {

        void Process(UIElement element, UITemplate template, ElementAttribute currentAttr, IReadOnlyList<ElementAttribute> attributes);

    }

    public class RouteAttrProcessor : IAttributeProcessor {

        public void Process(UIElement element, UITemplate template, ElementAttribute currentAttr, IReadOnlyList<ElementAttribute> attributes) {
            if (currentAttr.name != "route") return;

            if (currentAttr.name == "router") {
                CreateRouter(element, template, currentAttr, attributes);
            }
            else if (currentAttr.name == "route") {

                Router.ResolveRouterName(currentAttr.value, out string routerName, out string path);
                
                Route route = new Route(path);
                
                if (TryGetAttribute("onRouteEnter", attributes, out ElementAttribute onRouteEnterAttr)) {
                    
                }
            
                if (TryGetAttribute("onRouteEnter", attributes, out ElementAttribute onRouteChangedAttr)) {
                
                }
            
                if (TryGetAttribute("onRouteEnter", attributes, out ElementAttribute onRouteExitAttr)) {
                
                }

                Router router = Router.Find(element, routerName);
                
                router.AddRoute(route);
                
            }
            
            // todo the child attrs are called before the parent ones, in this case we want to reverse that
            
        }

        private void CreateRouter(UIElement element, UITemplate template, ElementAttribute currentAttr, IReadOnlyList<ElementAttribute> attributes) {

            Router router = Router.Create(element, currentAttr.value);
            
            if (TryGetAttribute("defaultRoute", attributes, out ElementAttribute defaultRouteAttr)) {
                    
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