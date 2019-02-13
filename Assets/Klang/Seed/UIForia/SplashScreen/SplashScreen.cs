using UIForia;
using UIForia.Animation;
using UIForia.Rendering;
using UIForia.Routing2;
using UnityEngine;

namespace UI {

    [Template(Root.BasePath + "SplashScreen/SplashScreen")]
    public class SeedSplashScreen : UIElement {

        private float elapsed;
        
        public override void OnReady() {
            FindById("logo").style.PlayAnimation(AnimateLogo());    
        }

        public override void OnUpdate() {
            elapsed += Time.deltaTime;
            if (elapsed >= 1f) {
                Application.RoutingSystem.FindRouter("game").GoTo("/login_flow");
            }

        }
        
        private static StyleAnimation AnimateLogo() {
            AnimationOptions options = new AnimationOptions();
            options.duration = 1f;
            
            return new PropertyAnimation(StyleProperty.TransformRotation(360f), options);    
        }
        
    }

}