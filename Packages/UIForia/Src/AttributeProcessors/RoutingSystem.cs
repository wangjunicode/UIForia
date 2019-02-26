using System;
using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Routing2;
using UIForia.Util;

namespace UIForia {

    public class RoutingSystem : ISystem {

        private readonly LightList<Router> m_Routers;
        private readonly List<ElementAttribute> m_ScratchAttrList = new List<ElementAttribute>();

        public RoutingSystem() {
            this.m_Routers = new LightList<Router>();
        }
        
        public void OnReset() {
            m_Routers.Clear();    
        }

        public void OnUpdate() {
            for (int i = 0; i < m_Routers.Count; i++) {
                m_Routers[i].Tick();
            }
        }

        public void OnDestroy() {}

        public void OnViewAdded(UIView view) {}

        public void OnViewRemoved(UIView view) {}

        public void OnElementEnabled(UIElement element) {}

        public void OnElementDisabled(UIElement element) {}

        public void OnElementDestroyed(UIElement element) {}

        public void OnAttributeSet(UIElement element, string attributeName, string currentvalue, string attributeValue) {}
        
        public void OnElementCreated(UIElement element) {

            m_ScratchAttrList.Clear();
            element.GetAttributes(m_ScratchAttrList);

            if (m_ScratchAttrList.Count == 0) {
                if (element.children != null) {
                    UIElement[] children = element.children.Array;
                    for (int i = 0; i < element.children.Count; i++) {
                        OnElementCreated(children[i]);
                    }
                }
                return;
            }
            
            if (TryGetAttribute("router", m_ScratchAttrList, out ElementAttribute routerAttr)) {
                TryGetAttribute("defaultRoute", m_ScratchAttrList, out ElementAttribute defaultRouteAttr);

                Router router = new Router(element.id, routerAttr.value, defaultRouteAttr.value);

                for (int i = 0; i < m_Routers.Count; i++) {
                    if (m_Routers[i].name == router.name) {
                        throw new Exception("Duplicate router defined with the name: " + router.name);
                    }
                }
                
                m_Routers.Add(router);
                
            }
            
            else if (TryGetAttribute("route", m_ScratchAttrList, out ElementAttribute routeAttr)) {
                
                Router.ResolveRouterName(routeAttr.value, out string routerName, out string path);

                Route route = new Route(path, element);

                if (TryGetAttribute("onRouteEnter", m_ScratchAttrList, out ElementAttribute onRouteEnterAttr)) { }

                if (TryGetAttribute("onRouteEnter", m_ScratchAttrList, out ElementAttribute onRouteChangedAttr)) { }

                if (TryGetAttribute("onRouteEnter", m_ScratchAttrList, out ElementAttribute onRouteExitAttr)) { }

                Router router = null;
                if (routerName == null) {
                    router = FindRouterInHierarchy(element);
                    if (router == null) {
                        throw new Exception("Cannot resolve router in hierarchy");
                    }
                }
                else {
                    router = FindRouter(routerName);
                    if (router == null) {
                        throw new Exception("Cannot resolve router with name: " + routerName);
                    }
                }
              
                router.AddRoute(route);

                element.SetEnabled(false);
            }
            
            if (element.children != null) {
                UIElement[] children = element.children.Array;
                for (int i = 0; i < element.children.Count; i++) {
                    OnElementCreated(children[i]);
                }
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

        public Router FindRouterInHierarchy(UIElement element) {
            IHierarchical ptr = element;
            while (ptr != null) {
                for (int i = 0; i < m_Routers.Count; i++) {
                    if (m_Routers[i].hostId == ptr.UniqueId) {
                        return m_Routers[i];
                    }
                }

                ptr = ptr.Parent;
            }

            return null;
        }
        
        public Router FindRouter(string routerName) {
            for (int i = 0; i < m_Routers.Count; i++) {
                if (m_Routers[i].name == routerName) {
                    return m_Routers[i];
                }
            }

            return null;
        }

    }

}