namespace UIForia {

    public class UIUnmatchedRoute : UIRouteElement {

        public UIUnmatchedRoute() : base(null) { }

        public override bool TryMatch(RouteMatch match, out RouteMatch result) {
            result = RouteMatch.Match(path, match);
            result.matchProgress = result.url.Length - 1;
            return true;
        }
        
    }

}