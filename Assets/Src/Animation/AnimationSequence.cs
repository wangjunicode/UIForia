using UnityEngine;

namespace Src.Animation {

    public class AnimationSequence : StyleAnimation {

        private StyleAnimation[] animations;
        private int currentAnimation;
        
        public AnimationSequence(AnimationOptions options, StyleAnimation[] animations) {
            this.options = options;
            this.animations = animations;
        }
        
        public override void Update(UIElement element, Rect viewport, float deltaTime) {

            float progress = 1f / options.duration;
            int count = animations.Length;
            float slice = count / options.duration;
            int currentIndex = 0;
            float smallestDist = float.MaxValue;

            for (int i = 0; i < count; i++) {
                float dist = Mathf.Abs((slice * i) - progress);
                if (dist < smallestDist) {
                    smallestDist = dist;
                    currentIndex = i;
                }
            }
            
            animations[currentIndex].Update(element, viewport, deltaTime);

        }

    }

}