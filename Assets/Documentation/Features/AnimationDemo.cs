using System;
using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.Features {

    [Template("Documentation/Features/AnimationDemo.xml")]
    public class AnimationDemo : UIElement {

        private UIElement animationTarget;

        public AnimationData animationData;
        
        public Action<int> OnDurationChanged => duration => animationData.options.duration = duration;

        public AnimationTask animationTask;

        public override void OnCreate() {
            animationTarget = FindById("animation-target");
        }

        public void ChangeAnimation(string animation) {
            animationData = Application.GetAnimationFromFile("Documentation/Features/AnimationDemo.style", animation);
            animationTarget.SetAttribute("anim", animation);
        }

        public void RunAnimationAgain() {
            animationTask = Application.Animate(animationTarget, animationData);
        }

        public void PauseAnimation() {
            Application.PauseAnimation(animationTarget, animationData);   
        }

        public void ResumeAnimation() {
            Application.ResumeAnimation(animationTarget, animationData);
        }
    }

}