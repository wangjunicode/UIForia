using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace SpaceGameDemo {

    [Template("SpaceGameDemo/SpaceGameRoot.xml")]
    public class SpaceGameRoot : UIElement {


        public float progress;
        private float speed = 0.1f;
        public string progressString;
        
        public override void OnCreate() {
        }

        public void OnSinglePlayerClick() {
            // gameCamera.
            
        }

        public override void OnUpdate() {
            progress += Time.deltaTime * speed;
            if (progress > 1) {
                progress = 0;
            }

            progressString = (progress * 100).ToString("F1");
        }

        public void OnAnimationFrame(AnimationState state) {
            
        }

    }

}
