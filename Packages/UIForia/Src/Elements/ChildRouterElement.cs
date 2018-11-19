using System.Collections.Generic;
using UIForia.Routing;
using UIForia.Util;

namespace UIForia {

    public class ChildRouterElement : UIRouteElement {

        public bool allowMultipleMatches;
        protected LightList<UIRouteElement> m_ChildRoutes;

        public ChildRouterElement(string path) : base(path) { }

        public override void OnReady() {
//            m_ChildRoutes = FindByType(LightListPool<RouteElement>.Get());
        }

        public override void Enter(RouteMatch route) {
            UIRouteElement[] routes = m_ChildRoutes.List;
            for (int i = 0; i < m_ChildRoutes.Length; i++) {
                RouteMatch childMatch;
                if (routes[i].TryMatch(route, out childMatch)) {
                    routes[i].Enter(childMatch);
                    break;
                }
            }
        }

    }

}