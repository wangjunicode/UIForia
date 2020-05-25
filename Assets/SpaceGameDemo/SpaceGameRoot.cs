using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace SpaceGameDemo {

    [Template("SpaceGameDemo/SpaceGameRoot.xml")]
    public class SpaceGameRoot : UIElement {

        public Camera gameCamera;

        public override void OnCreate() {
            gameCamera = GameObject.Find("Look Camera").GetComponent<Camera>();
        }

        public void OnSinglePlayerClick() {
            // gameCamera.
            
        }

        public void OnAnimationFrame(AnimationState state) {
            
        }

    }

}
