using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEditor;
using UnityEngine;

namespace Documentation.DocumentationElements {

    [Template("Documentation/DocumentationElements/Anchor")]
    public class Anchor : UIElement {

        private static Color highlight = new Color(0.1f, 0.39f, 0.5f);
        
        public string href;

        public override void OnUpdate() {
            if (Application.RoutingSystem.FindRouter("demo").CurrentUrl == href) {
                style.SetBackgroundColor(highlight, StyleState.Normal);
                style.SetTextColor(Color.white, StyleState.Normal);
            }
            else {
                style.SetBackgroundColor(Color.clear, StyleState.Normal);
                style.SetTextColor(Color.black, StyleState.Normal);
                
            }
        }

        [OnMouseClick()]
        public void OnClick() {
            Application.RoutingSystem.FindRouter("demo").GoTo(href);
        }
    }
}
