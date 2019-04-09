using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Animation {

    public class AnimationSequence : StyleAnimation {

        private StyleAnimation[] animations;
        private int currentAnimation;
        
        public AnimationSequence(AnimationOptions mOptions, StyleAnimation[] animations) {
            this.m_Options = mOptions;
            this.animations = animations;
        }
        
        public override AnimationStatus Update(UIStyleSet styleSet, Rect viewport, float deltaTime) {

            float progress = 1f / m_Options.duration.Value;
            int count = animations.Length;
            float slice = count / m_Options.duration.Value;
            int currentIndex = 0;
            float smallestDist = float.MaxValue;

            for (int i = 0; i < count; i++) {
                float dist = Mathf.Abs((slice * i) - progress);
                if (dist < smallestDist) {
                    smallestDist = dist;
                    currentIndex = i;
                }
            }
            
            animations[currentIndex].Update(styleSet, viewport, deltaTime);

            return AnimationStatus.Running;
        }

        public override void OnStart(UIStyleSet styleSet, Rect viewport) {
            
        }

    }

}