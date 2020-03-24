using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace UI {

    [Template("WindowSample.xml")]
    public class WindowSample : UIElement {

        public void SpawnWindow() {
//            Application.CreateView("Window", new Rect(0, 0, 300, 300), typeof(KlangWindow));
        }

    }

}