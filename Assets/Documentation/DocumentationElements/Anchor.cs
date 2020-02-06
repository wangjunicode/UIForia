using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace Documentation.DocumentationElements {

    public class Anchor : UIContainerElement {

        private static Color highlight = new Color(0.1f, 0.39f, 0.5f);
        
        public string href;

        public override void OnUpdate() {
            if (application.RoutingSystem.FindRouter("demo").CurrentUrl == href) {
                style.SetBackgroundColor(highlight, StyleState.Normal);
                style.SetTextColor(Color.white, StyleState.Normal);
                style.SetTextColor(Color.black, StyleState.Hover);
            }
            else {
                style.SetBackgroundColor(Color.clear, StyleState.Normal);
                style.SetTextColor(Color.black, StyleState.Normal);
            }
        }

        [OnMouseClick()]
        public void OnClick() {
            application.RoutingSystem.FindRouter("demo").GoTo(href);
        }
    }
}
