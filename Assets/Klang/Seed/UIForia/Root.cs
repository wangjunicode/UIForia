using UIForia;

namespace UI {

    [Template("Klang/Seed/UIForia/Root.xml")]
    public class Root : UIElement {

        public override void OnCreate() {
            view.Application.Router.GoTo("/game");
        }

        public void StartGame() {
            SeedDebugWindow window1 = CreateChild<SeedDebugWindow>();
            SeedDebugWindow window2 = CreateChild<SeedDebugWindow>();
            SeedDebugWindow window3 = CreateChild<SeedDebugWindow>();
        }

    }

}