using UIForia;
using UIForia.Animation;
using UIForia.Rendering;
using UIForia.Routing;
using UnityEngine;

namespace UI {

    [Template("Klang/Seed/UIForia/LoginFlow/CreateAccountScreen/CreateAccountScreen.xml")]
    public class CreateAccountScreen : UIElement {

        public string username;
        public string password;
        public string colonyName;
        public string colonyPassword;
        
        private UIElement fadePanel;
        private ChildRouterElement childRouter;

        public override void OnReady() {
            fadePanel = FindById("fader");
        }

        public void AnimateTransition() {
            fadePanel.style.SetVisibility(Visibility.Visible, StyleState.Normal);
            fadePanel.style.PlayAnimation(FadeAnimation());
        }

        private static StyleAnimation FadeAnimation() {
            AnimationOptions options = new AnimationOptions();
            options.duration = 1f;
            return new KeyFrameAnimation(options,
                new AnimationKeyFrame(0.0f, StyleProperty.BackgroundColor(new Color(0, 0, 0, 0))),
                new AnimationKeyFrame(0.5f, StyleProperty.BackgroundColor(Color.black)),
                new AnimationKeyFrame(1.0f, StyleProperty.BackgroundColor(new Color(0, 0, 0, 0)))
            );
        }

    }

}