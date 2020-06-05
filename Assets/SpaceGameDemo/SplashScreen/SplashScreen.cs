using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace SpaceGameDemo.SplashScreen {
    
    [Template("SpaceGameDemo/SplashScreen/SplashScreen.xml")]
    public class SplashScreen : UIElement {
        // Property
        public float progress;

        private bool timeIsUp;
        
        public override void OnUpdate() {
            if (timeIsUp) {
                return;
            }

            progress += Time.deltaTime;
            if (progress > 4) {
                timeIsUp = true;
                SetAttribute("animate", "fadeOut");
            }
        }
    }
}