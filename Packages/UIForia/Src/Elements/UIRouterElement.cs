using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Routing;

namespace UIForia {

    public class UIRouterElement : UIContainerElement, IRouteHandler {

        public readonly string path;
        public event Action onRouteEnter;
        public event Action onRouteExit;
        public event Action onRouteChanged;

        private UIRouteElement current;
        private readonly List<UIRouteElement> m_Routes;
        public string initial;

        public UIRouterElement(string path) {
            this.path = path;
            this.m_Routes = new List<UIRouteElement>();
         //   flags &= ~(UIElementFlags.RequiresLayout | UIElementFlags.RequiresLayout);
            // todo -- validate child routes, can't re-use dynamic segment names

        }

        public override void OnCreate() {
            FindByType(m_Routes); // todo -- should not be template scoped
            for (int i = 0; i < m_Routes.Count; i++) {
                
                m_Routes[i].SetEnabled(m_Routes[i].IsRouteMatched);
                
                IRouteGuard guard = m_Routes[i] as IRouteGuard;
                if (guard != null) {
                    view.Application.Router.AddRouteGuard(guard);
                }
            }

            if (initial != null) {
                OnRouteChanged(new Route(initial));
            }
        }

        public void OnRouteChanged(Route route) {
            RouteMatch match = RouteMatch.Match(path, new RouteMatch(route.path));
            if (!match.IsMatch) {
                return;
            }

            for (int i = 0; i < m_Routes.Count; i++) {

                RouteMatch childMatch;
                if (m_Routes[i].TryMatch(match, out childMatch)) {
                    if (m_Routes[i] == current) {
                        m_Routes[i].Update();
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