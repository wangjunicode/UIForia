using System;
using UIForia.Elements;
using UIForia.Routing;

namespace UIForia {

    [TemplateTagName("Route")]
    public class RouteElement : UIContainerElement {

        public string path;
        public event Action onEnter;
        public event Action onExit;
        public event Action onUpdate;
        protected RouteMatch match;
        
        public RouteMatch CurrentMatch => match;
        public bool IsRouteMatched => match.IsMatch;
        public virtual string FullPath => path;

        public RouteElement() {
            flags &= ~(UIElementFlags.RequiresLayout | UIElementFlags.RequiresRendering);
        }

        public override void OnCreate() {
            UIElement ptr = parent;
            while (ptr != null) {
                if (ptr is IRouterElement routerElement) {
                    routerElement.AddChildRoute(this);
                    break;
                }

                ptr = ptr.parent;
            }
        }

        public override void OnDestroy() {
            UIElement ptr = parent;
            while (ptr != null) {
                if (ptr is IRouterElement routerElement) {
                    routerElement.RemoveChildRoute(this);
                    break;
                }

                ptr = ptr.parent;
            }
        }

        public virtual bool TryMatch(RouteMatch match, out RouteMatch result) {
            if (path == null) {
                result = default;
                result.matchProgress = -1;
                return false;
            }
            result = RouteMatch.Match(path, match);
            return result.IsMatch;
        }

        public virtual void Enter(RouteMatch match) {
            this.match = match;
            onEnter?.Invoke();
            SetEnabled(true);
        }

        public virtual void Exit() {
            onExit?.Invoke();
            match = default;
            match.matchProgress = -1;
            SetEnabled(false);
        }

        public void Update(RouteMatch match) {
            this.match = match;
            onUpdate?.Invoke();
        }

    }

}