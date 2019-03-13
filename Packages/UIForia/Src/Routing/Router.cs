using System;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Routing {

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
        public UIElement element;
        public event Action onRouteEnter;
        public event Action onRouteExit;
        public event Action onRouteUpdate;

        public Route(string path, UIElement element) {
            this.path = path;
            this.element = element;
        }

        public void Enter() {
            element.SetEnabled(true);
            onRouteEnter?.Invoke();
        }

        public void Exit() {
            element.SetEnabled(false);
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
                
                if (current == '*') return new RouteMatch(matchPath, matchPtr);

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
        private bool isInitialized;
        private int historyIndex;

        private readonly LightList<Route> m_HistoryStack;
        private readonly LightList<Route> m_RouteHandlers;
        private readonly LightList<RouteTransition> m_Transitions;
        private readonly LightList<RouteTransition> m_ActiveTransitions;

        private Route activeRoute;
        private Route targetRoute;

        private string targetUrl;
        private string defaultRoute;

        internal Router(int hostId, string name, string defaultRoute = null) {
            this.hostId = hostId;
            this.name = name;
            this.defaultRoute = defaultRoute;
            
            m_HistoryStack = new LightList<Route>();
            m_RouteHandlers = new LightList<Route>();
            m_Transitions = new LightList<RouteTransition>();
            m_ActiveTransitions = new LightList<RouteTransition>();

            activeRoute = null;
            targetRoute = null;
        }
        
        public bool CanGoForwards => m_HistoryStack.Count > 1 && historyIndex != m_HistoryStack.Count - 1;

        public void GoBack() {
            if (historyIndex == 0) {
                return;
            }

            m_HistoryStack.RemoveLast();
            Route route = m_HistoryStack.RemoveLast();
            historyIndex = historyIndex - 2;
            GoTo(route.path);
        }

        public Route ActiveRoute => activeRoute;
        public Route TargetRoute => targetRoute;
        public string CurrentUrl { get; private set; }

        public void AddRoute(UIElement element, string path) {
            this.m_RouteHandlers.Add(new Route(path, element));
        }

        public void AddRoute(Route route) {
            this.m_RouteHandlers.Add(route);
        }

        public void AddTransition(string fromPath, string toPath, Func<float, RouteTransitionState> action) {
            m_Transitions.Add(new RouteTransition(fromPath, toPath, action));
        }

        public void Initialize() {
            if (isInitialized) return;
            isInitialized = true;

            if (defaultRoute != null) {
                CurrentUrl = defaultRoute;
                targetUrl = defaultRoute;

                for (int i = 0; i < m_RouteHandlers.Count; i++) {
                    RouteMatch match = Route.Match(m_RouteHandlers[i].path, defaultRoute, 0);
                    if (match.IsMatch) {
                        targetRoute = m_RouteHandlers[i];

                        if (m_ActiveTransitions.Count == 0) {
                            EnterRoute();
                        }

                        return;
                    }
                }
            }
        }

        private float tickElapsed;
        
        public void Tick() {
            Initialize();

            if (m_ActiveTransitions.Count == 0) {
                return;
            }

            for (int i = 0; i < m_ActiveTransitions.Count; i++) {
                RouteTransitionState state = m_ActiveTransitions[i].fn(tickElapsed);

                if (state == RouteTransitionState.Cancelled) {
                    // reset transition state
                    m_ActiveTransitions.Clear();
                    return;
                }

                if (state == RouteTransitionState.Completed) {
                    m_ActiveTransitions.RemoveAt(i--);
                }
            }

            if (m_ActiveTransitions.Count == 0) {
                EnterRoute();
            }

            tickElapsed += Time.deltaTime;
        }

        // todo if goto is called during an active transition the old route will not be disabled leaving two routes active at the same time
        public void GoTo(string path) {
            targetUrl = path;
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

            historyIndex++;
            m_HistoryStack.Add(activeRoute);
            tickElapsed = 0;
            CurrentUrl = targetUrl;
        }

        private void GatherTransitions(string path) {
            for (int i = 0; i < m_Transitions.Count; i++) {
                if (IsTransitionApplicable(m_Transitions[i], path)) {
                    m_ActiveTransitions.Add(m_Transitions[i]);
                }
            }
        }

        private bool IsTransitionApplicable(RouteTransition transition, string path) {
            bool first = Route.Match(transition.fromPath, CurrentUrl, 0).IsMatch;
            bool second = Route.Match(transition.toPath, path, 0).IsMatch;
            return first && second;
        }

        private bool IsTransitionAllowed(string path, string targetPath) {
            return true;
        }

        public static void ResolveRouterName(string value, out string routerName, out string path) {
            int idx = value.IndexOf("::", StringComparison.Ordinal);
            if (idx != -1) {
                routerName = value.Substring(0, idx);
                path = value.Substring(idx + 2);
            }
            else {
                routerName = null;
                path = value;
            }
        }

    }

}
