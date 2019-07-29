using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;
using UnityEngine.UI;


namespace Documentation.Features {
    [Template("Documentation/Features/Test.xml")]
    public class Test : UIElement {
        private UIElement button;
        
       
        public override void OnCreate() {
            button = FindById("text");
        }
        
        public void OnButtonClick() {
            button.style.SetBackgroundColor(new Color(0.2f, 0.2f, 0.2f, 1f), StyleState.Normal);
            Debug.Log("Hi");
        }
    }
}