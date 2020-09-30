using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace SpaceGameDemo.SplashScreen {
    
    [Template("SplashScreen/SplashScreen.xml")]
    public class SplashScreen : UIElement {
        
        // Parameter
        public float hideAfter = 5f;
        
        // Property
        public float progress;
        
        // Property
        public string animate;

        private bool timeIsUp;

        public override void OnEnable() {
            animate = string.Empty;
        }

        public override void OnUpdate() {
            if (timeIsUp) {
                return;
            }

            progress += Time.deltaTime;
            if (progress > hideAfter) {
                timeIsUp = true;
                animate = "fadeOut";
            }
        }
    }
}