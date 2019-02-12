using UIForia;
using UIForia.Animation;
using UIForia.Rendering;
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
            if (elapsed >= 1000f) {
             //   view.Application.GetRouter("game").SetRoute("/LoginFlow");
            }
        }
        
        private static StyleAnimation AnimateLogo() {
            AnimationOptions options = new AnimationOptions();
            options.duration = 1f;
            
            return new PropertyAnimation(StyleProperty.TransformRotation(360f), options);    
        }
        
    }

}