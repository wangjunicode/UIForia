using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Routing;

namespace UIForia {

    [TemplateTagName("Router")]
    public class RouterElement : UIContainerElement, IRouteHandler, IRouterElement {

        public event Action onRouteEnter;
        public event Action onRouteExit;
        public event Action onRouteChanged;

        private RouteElement current;
        private readonly List<RouteElement> m_Routes;

        public RouterElement() {
            this.m_Routes = new List<RouteElement>();
           // flags &= ~(UIElementFlags.RequiresLayout | UIElementFlags.RequiresRendering);
            // todo -- validate child routes, can't re-use dynamic segment names
        }

        public override void OnReady() {
//            style.SetPreferredWidth(UIMeasurement.Parent100, StyleState.Normal);
//            style.SetPreferredHeight(UIMeasurement.Parent100, StyleState.Normal);
            OnRouteChanged(new Route(view.Application.Router.CurrentUrl));
        }

        public void AddChildRoute(RouteElement route) {
            if (!m_Routes.Contains(route)) {
                m_Routes.Add(route);
                route.SetEnabled(false);
            }
        }

        public void RemoveChildRoute(RouteElement route) {
            m_Routes.Remove(route);
        }

        public void OnRouteChanged(Route route) {

            RouteMatch match = new RouteMatch(route.path);
            
            for (int i = 0; i < m_Routes.Count; i++) {

                RouteMatch childMatch;
                if (m_Routes[i].TryMatch(match, out childMatch)) {
                    if (m_Routes[i] == current) {
                        m_Routes[i].Update(childMatch);
                        onRouteChanged?.Invoke();
                        return;
                    }
                    else {
                        if (current != null) {
                            current.Exit();
                            onRouteExit?.Invoke();
                        }
                        m_Routes[i].Enter(childMatch);
                        onRouteEnter?.Invoke();
                        current = m_Routes[i];
                        return;
                    }

                }
                
            }
            
            if (current != null) {
                current.Exit();
                onRouteExit?.Invoke();
            }

        }

    }

}