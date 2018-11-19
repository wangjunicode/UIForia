using System;
using UIForia.Elements;
using UIForia.Routing;

namespace UIForia {

    public class UIRouteElement : UIContainerElement {

        public readonly string path;
        public event Action onEnter;
        public event Action onExit;
        public event Action onUpdate;
        private RouteMatch match;
        protected bool isMatched;
        
        public RouteMatch CurrentMatch => match;
        public bool IsRouteMatched => isMatched;

        public UIRouteElement(string path) {
            this.path = path;
        }

        public virtual bool TryMatch(RouteMatch match, out RouteMatch result) {
            result = RouteMatch.Match(path, match);
            return result.IsMatch;
        }

         // push $routeParameters.xxx into context on binding
        public virtual void Enter(RouteMatch match) {
            this.match = match;
            isMatched = true;
            onEnter?.Invoke();
            SetEnabled(true);
        }

        public virtual void Exit() {
            onExit?.Invoke();
            isMatched = false;
            SetEnabled(false);
        }

        public void Update() {
            onUpdate?.Invoke();
        }

    }

}