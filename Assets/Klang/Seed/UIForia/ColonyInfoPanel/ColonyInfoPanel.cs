using UIForia;
using UIForia.Rendering;
using UnityEngine;

namespace UI {

    [Template("Klang/Seed/UIForia/ColonyInfoPanel/ColonlyInfoPanel.xml")]
    public class ColonyInfoPanel : UIElement {

        private ColonyInfoWindow window;

        public override void OnCreate() {
            ShowInfoWindow();
        }

        public void ShowInfoWindow() {
            if (window != null) return;
            window = CreateChild<ColonyInfoWindow>();
            
            window.style.SetTransformPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), StyleState.Normal);
            
        }

    }

}