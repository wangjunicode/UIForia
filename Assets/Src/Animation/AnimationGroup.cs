using UnityEngine;

namespace Src.Animation {

    public class AnimationGroup : StyleAnimation {

        private StyleAnimation[] animations;
        
        public AnimationGroup(StyleAnimation[] animations) {
            this.animations = animations;
        }
        
        public override void Update(UIElement element, Rect viewport, float deltaTime) {
            for (int i = 0; i < animations.Length; i++) {
                animations[i].Update(element, viewport, deltaTime);
            }
        }

    }

}