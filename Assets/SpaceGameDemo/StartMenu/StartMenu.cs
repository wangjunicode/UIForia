using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace SpaceGameDemo.StartMenu {
    
    [Template("SpaceGameDemo/StartMenu/StartMenu.xml")]
    public class StartMenu : UIElement {
        // Property
        public float progress;

        // Property
        public string progressString;

        private float speed = 0.1f;

        public override void OnUpdate() {
            progress += Time.deltaTime * speed;
            if (progress > 1) {
                progress = 0;
            }

            progressString = (progress * 100).ToString("F1");
        }
    }
}