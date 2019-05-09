using UIForia.Animation;
using UIForia.Rendering;

namespace UIForia.Compilers.Style {

    public class StyleSheet {

        internal UIStyleGroupContainer[] styleGroupContainers;

        internal StyleConstant[] constants;
        internal AnimationData[] animations;

        internal StyleSheet(StyleConstant[] constants, UIStyleGroupContainer[] styleGroupContainers, AnimationData[] animations) {
            this.constants = constants;
            this.styleGroupContainers = styleGroupContainers;
            this.animations = animations;
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