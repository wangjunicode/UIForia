using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

#pragma warning disable 0414
namespace Documentation.Features {

    [Template("Documentation/Features/GridDemo.xml")]
    public class GridDemo : UIElement {

        public string variableStyle = "varStyle0";

        private string backgroundStyleName = "background";
        private bool hover;

        public void OnHover() {
            hover = true;
        }

        public void OnHoverEnd() {
            hover = false;
        }

        public override void OnUpdate() {
            Debug.Log("Updated");    
        }
        
        [OnMouseClick]
        public void OnMouseClick() {
            if (variableStyle == string.Empty) {
                variableStyle = "varStyle0";
            }
            else {
                variableStyle = string.Empty;
            }
        }

    }

}