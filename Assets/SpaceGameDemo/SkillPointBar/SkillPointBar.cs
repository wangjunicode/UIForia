using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;

namespace SpaceGameDemo.SkillPointBar {
    
    [Template("SpaceGameDemo/SkillPointBar/SkillPointBar.xml")]
    public class SkillPointBar : UIElement {

        public int availablePoints;

        public int skillPoints;

        public void IncreaseSkill() {
            if (skillPoints < 25 && availablePoints > 0) {
                skillPoints++;
                availablePoints--;
            }
            else {
                UIElement skillBar = FindById("skill-bar");
                skillBar.Animator.TryGetAnimationData("warning", out AnimationData animationData);
                skillBar.Animator.PlayAnimation(animationData);
            }
        }

        public void DecreaseSkill() {
            if (skillPoints > 1) {
                skillPoints--;
                availablePoints++;
            }
            else {
                UIElement skillBar = FindById("skill-bar");
                skillBar.Animator.TryGetAnimationData("warning", out AnimationData animationData);
                skillBar.Animator.PlayAnimation(animationData);
            }
        }
    }
}