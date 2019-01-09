using UIForia;
using UIForia.Animation;
using UIForia.Rendering;
using UnityEngine;

namespace UI {

    [Template("Klang/Seed/UIForia/SkillbarDisplay/SkillbarDisplay.xml")]
    public class SkillbarDisplay : UIElement {

        public string label;
        public float progress;
        public int level;
        public string tooltip;
        private float animationTime;
        
        public override void OnEnable() {
            FindById("foreground").style.PlayAnimation(AnimateProgressBar(progress));   
        }
        
        public static StyleAnimation AnimateProgressBar(float progress) {
            AnimationOptions options = new AnimationOptions();
            options.duration = 0.35f;
            options.timingFunction = EasingFunction.Linear;
            StyleProperty start = StyleProperty.PreferredWidth(0f);
            StyleProperty target = StyleProperty.PreferredWidth(new UIMeasurement(progress, UIMeasurementUnit.ParentSize));
            return new PropertyAnimation(start, target, options);
        }

    }

}