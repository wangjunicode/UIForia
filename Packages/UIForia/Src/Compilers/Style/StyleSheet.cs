using UIForia.Animation;
using UIForia.Rendering;
using UIForia.Sound;

namespace UIForia.Compilers.Style {

    public class StyleSheet {

        internal UIStyleGroupContainer[] styleGroupContainers;

        internal StyleConstant[] constants;
        internal AnimationData[] animations;
        internal UISoundData[] sounds;

        internal StyleSheet(StyleConstant[] constants, UIStyleGroupContainer[] styleGroupContainers, AnimationData[] animations, UISoundData[] sounds) {
            this.constants = constants;
            this.styleGroupContainers = styleGroupContainers;
            this.animations = animations;
            this.sounds = sounds;
        }

        public bool TryGetAnimationData(string name, out AnimationData retn) {
            for (int i = 0; i < animations.Length; i++) {
                if (animations[i].name == name) {
                    retn = animations[i];
                    return true;
                }
            }

            retn = default;
            return false;
        }

        public UIStyleGroupContainer GetStyleGroupsByTagName(string tagName) {
            for (int i = 0; i < styleGroupContainers.Length; i++) {
                UIStyleGroupContainer container = styleGroupContainers[i];
                if (container.styleType == StyleType.Implicit && container.name == tagName) {
                    return container;
                }
            }

            return null;
        }

        public UIStyleGroupContainer GetStyleGroupByStyleName(string styleName) {
            for (int i = 0; i < styleGroupContainers.Length; i++) {
                UIStyleGroupContainer container = styleGroupContainers[i];
                if (container.styleType == StyleType.Shared && container.name == styleName) {
                    return container;
                }
            }

            return null;
        }

    }

}