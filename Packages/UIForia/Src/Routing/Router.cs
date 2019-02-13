using System;
using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Routing2 {

    public enum RouteTransitionState {

        Pending,
        Completed,
        Cancelled

    }

    public struct RouteTransition {

        public readonly string fromPath;
        public readonly string toPath;
        public readonly Func<float, RouteTransitionState> fn;

        public RouteTransition(string fromPath, string toPath, Func<float, RouteTransitionState> fn) {
            this.fromPath = fromPath;
            this.toPath = toPath;
            this.fn = fn;
        }

    }

    public struct RouteMatch {

        public readonly string url;
        public readonly int matchProgress;

        public RouteMatch(string url, int matchProgress = -1) {
            this.url = url;
            this.matchProgress = matchProgress;
        }

        public bool IsMatch => matchProgress >= 0;

    }

    public class Route {

        public string path;
        public event Action onRouteEnter;
        public event Action onRouteExit;
        public event Action onRouteUpdate;

        // path       = /game/view/creator/1/eyebrows
        // match Path = /game/view/character

        public Route(string path) {
            this.path = path;
        }

        public void Enter() {
            onRouteEnter?.Invoke();
        }

        public void Exit() {
            onRouteExit?.Invoke();
        }

        public void Update() {
            onRouteUpdate?.Invoke();
        }

        public static RouteMatch Match(string path, string matchPath, int matchPtr) {
            if (path == "*") {
                return new RouteMatch(matchPath, 0);
            }

            int ptr = 0;

            while (ptr < path.Length) {
                if (matchPtr >= matchPath.Length) {
                    return new RouteMatch(matchPath, -1);
                }

                if (ptr >= path.Length) {
                    return new RouteMatch(matchPath, -1);
                }

                char current = path[ptr];
                char matchCurrent = matchPath[matchPtr];

                if (current == ':') {
                    throw new NotSupportedException();
                }

                if (current == matchCurrent) {
                    ptr++;
                    matchPtr++;
                    continue;
                }

                return new RouteMatch(matchPath, -1);
            }

            if (matchPtr - 1 != matchPath.Length - 1) {
                return new RouteMatch(matchPath, -1);
            }

            return new RouteMatch(matchPath, matchPtr);
        }

    }

    public class Router {

        public int hostId;
        public readonly string name;

        private static readonly LightList<Router> s_Routers = new LightList<Router>();

        private readonly LightList<Route> m_RouteHandlers;
        private readonly LightList<RouteTransition> m_Transitions;
        private readonly LightList<RouteTransition> m_ActiveTransitions;

        private Route activeRoute;
        private Route targetRoute;

        public Router(int hostId, string name) {
            this.hostId = hostId;
            this.name = name;
            m_RouteHandlers = new LightList<Route>();
            m_Transitions = new LightList<RouteTransition>();
            m_ActiveTransitions = new LightList<RouteTransition>();

            activeRoute = null;
            targetRoute = null;
        }

        public Route ActiveRoute => activeRoute;
        public Route TargetRoute => targetRoute;

        public void AddRoute(string path) {
            this.m_RouteHandlers.Add(new Route(path));
        }

        public void AddRoute(Route route) {
            this.m_RouteHandlers.Add(route);
        }

        public void AddTransition(string fromPath, string toPath, Func<float, RouteTransitionState> action) {
            m_Transitions.Add(new RouteTransition(fromPath, toPath, action));
        }

        public static Router Find(string routerName) {
            for (int i = 0; i < s_Routers.Count; i++) {
                if (s_Routers[i].name == routerName) {
                    return s_Routers[i];
                }
            }

            return null;
        }

        public static Router Find(IHierarchical element, string routerName) {
            // todo find router by element hierarchy
            IHierarchical ptr = element;
            while (ptr != null) {
                for (int i = 0; i < s_Routers.Count; i++) {
                    if (s_Routers[i].hostId == ptr.UniqueId) {
                        return s_Routers[i];
                    }
                }

                ptr = ptr.Parent;
            }

            for (int i = 0; i < s_Routers.Count; i++) {
                if (s_Routers[i].name == routerName) {
                    return s_Routers[i];
                }
            }

            return null;
        }

        public void Tick() {
            float delta = Time.deltaTime;

            if (m_ActiveTransitions.Count == 0) {
                return;
            }

            for (int i = 0; i < m_ActiveTransitions.Count; i++) {
                RouteTransitionState state = m_ActiveTransitions[i].fn(delta);

                if (state == RouteTransitionState.Cancelled) {
                    // reset transition state
                    m_ActiveTransitions.Clear();
                    return;
                }

                if (state == RouteTransitionState.Completed) {
                    m_Transitions.RemoveAt(i--);
                }
            }

            if (m_ActiveTransitions.Count == 0) {
                EnterRoute();
            }
        }


        public void GoTo(string path) {
            for (int i = 0; i < m_RouteHandlers.Count; i++) {
                RouteMatch match = Route.Match(m_RouteHandlers[i].path, path, 0);
                if (match.IsMatch) {
                    if (!IsTransitionAllowed(path, match.url)) {
                        return;
                    }

                    targetRoute = m_RouteHandlers[i];

                    GatherTransitions(path);

                    if (m_ActiveTransitions.Count == 0) {
                        EnterRoute();
                    }

                    return;
                }
            }
        }

        private void EnterRoute() {
            if (targetRoute != activeRoute) {
                activeRoute?.Exit();
                targetRoute.Enter();
                activeRoute = targetRoute;
            }
            else {
                activeRoute.Update();
            }
        }

        private void GatherTransitions(string path) {
            for (int i = 0; i < m_Transitions.Count; i++) {
                if (IsTransitionApplicable(m_Transitions[i], path)) {
                    m_ActiveTransitions.Add(m_Transitions[i]);
                }
            }
        }

        private static bool IsTransitionApplicable(RouteTransition transition, string path) {
            return Route.Match(transition.fromPath, path, 0).IsMatch;
        }

        private bool IsTransitionAllowed(string path, string targetPath) {
            return true;
        }

        public static Router Create(UIElement element, string currentAttrValue) {
            // todo eventually we want to segment routers by application, ie 1 set for editor, 1 set for game
            Router router = new Router(element.id, currentAttrValue);
            s_Routers.Add(router);
            return router;
        }

        public static void ResolveRouterName(string value, out string routerName, out string path) {
            int idx = value.IndexOf("::", StringComparison.Ordinal);
            if (idx != -1) {
                routerName = value.Substring(0, idx);
                path = value.Substring(idx + 2);
            }
            else {
                routerName = "__default__";
                path = value;
            }
        }

    }

}

namespace UIForia.Routing {

    public struct Route {

        public string path;

        public Route(string path) {
            this.path = path;
        }

    }

    public interface IRouteHandler {

        void OnRouteChanged(Route route);

    }

    public interface IRouteGuard {

        bool CanTransition(Route current, Route next);

    }

    public class Router {

        private Route current;
        private int historyIndex;

        private readonly LightList<Route> m_HistoryStack;
        private readonly LightList<IRouteGuard> m_Guards;
        private readonly LightList<IRouteHandler> m_Handlers;

        public Router() {
            current = new Route("");
            m_HistoryStack = new LightList<Route>();
            m_Guards = new LightList<IRouteGuard>();
            m_Handlers = new LightList<IRouteHandler>();
            m_HistoryStack.Add(current);
        }

        public string CurrentUrl => current.path;
        public bool CanGoBack => historyIndex > 1;
        public bool CanGoForwards => m_HistoryStack.Count > 1 && historyIndex != m_HistoryStack.Count - 1;

        public void AddRouteHandler(IRouteHandler handler) {
            m_Handlers.Add(handler);
        }

        public void AddRouteGuard(IRouteGuard guard) {
            m_Guards.Add(guard);
        }

        public void GoForwards() { }

        public void GoBack() {
            if (historyIndex == 0) {
                return;
            }

            historyIndex--;
            Route route = m_HistoryStack[historyIndex];
            for (int i = 0; i < m_Handlers.Count; i++) {
                m_Handlers[i].OnRouteChanged(route);
            }

            current = route;
        }

        public bool GoTo(string path) {
            return GoTo(new Route(path));
        }

        public bool GoTo(Route route) {
            if (IsTransitionBlocked(route)) {
                return false;
            }

            for (int i = 0; i < m_Handlers.Count; i++) {
                m_Handlers[i].OnRouteChanged(route);
            }

            current = route;
            historyIndex++;
            m_HistoryStack.Add(route);

            return false;
        }

        private bool IsTransitionBlocked(Route route) {
            for (int i = 0; i < m_Guards.Count; i++) {
                if (!m_Guards[i].CanTransition(current, route)) {
                    return true;
                }
            }

            return false;
        }

    }

}