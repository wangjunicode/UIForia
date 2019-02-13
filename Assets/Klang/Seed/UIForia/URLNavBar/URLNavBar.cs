using UIForia;
using UIForia.Routing2;

namespace UI {

    [Template("Klang/Seed/UIForia/URLNavBar/URLNavBar.xml")]
    public class URLNavBar : UIElement {

        public string targetRouterName;
        private Router router;

        public override void OnReady() {
            router = Application.RoutingSystem.FindRouter(targetRouterName);
        }

        [OnPropertyChanged(nameof(targetRouterName))]
        private void OnTargetRouterChanged(string name) {
            router = Application.RoutingSystem.FindRouter(targetRouterName);
        }

    }

}