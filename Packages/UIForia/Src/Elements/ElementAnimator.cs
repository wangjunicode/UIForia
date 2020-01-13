using System;
using UIForia.Animation;
using UIForia.Elements;

namespace UIForia {

    public struct ElementAnimator {

        public readonly UIElement element;
        private readonly AnimationSystem animationSystem;

        internal ElementAnimator(AnimationSystem animationSystem, UIElement element) {
            this.element = element;
            this.animationSystem = animationSystem;
        }

        public AnimationTask Animate(UIElement element, AnimationData animation) {
            return animationSystem.Animate(element, ref animation);
        }

        public void PauseAnimation(UIElement element, AnimationData animationData) {
            animationSystem.PauseAnimation(element, ref animationData);
        }

        public void Resume(AnimationData animationData) {
            animationSystem.ResumeAnimation(element, ref animationData);
        }

        public void StopAnimation(UIElement element, AnimationData animationData) {
            animationSystem.StopAnimation(element, ref animationData);
        }

        public AnimationData GetAnimationFromFile(string fileName, string animationName) {
           throw new NotImplementedException("Re design this not to use style importer");
        }


    }

}