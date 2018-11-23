using UIForia;
using UIForia.Animation;
using UIForia.Rendering;
using UIForia.Routing;
using UnityEngine;

namespace UI.LoginFlow {

    [Template("Klang/Seed/UIForia/LoginFlow/CreateAccountScreen/CreateAccountScreen.xml")]
    public class CreateAccountScreen : UIElement {

        public string username;
        public string password;

        private UIElement fadePanel;
        private ChildRouterElement childRouter;

        public override void OnReady() {
            fadePanel = FindById("fader");
//            childRouter.TransitionMode = Animated
        }

        public void AnimateTransition() {
            fadePanel.style.PlayAnimation(FadeAnimation());
            //anim.onProgress(float progress) { if(progress >= 0.5 && !transitioned) childRouter.DoTransition }
        }

        private static StyleAnimation FadeAnimation() {
            AnimationOptions options = new AnimationOptions();
            options.duration = 1f;
            return new KeyFrameAnimation(options,
                new AnimationKeyFrame(0, StyleProperty.BackgroundColor(new Color(0, 0, 0, 0))),
                new AnimationKeyFrame(0, StyleProperty.BackgroundColor(Color.black)),
                new AnimationKeyFrame(0, StyleProperty.BackgroundColor(new Color(0, 0, 0, 0)))
            );
        }

    }

}