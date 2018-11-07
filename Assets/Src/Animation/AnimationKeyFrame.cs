using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Animation {

    public struct AnimationKeyFrame {

        public readonly float key;
        public readonly StyleProperty[] properties;

        public AnimationKeyFrame(float key, params StyleProperty[] properties) {
            this.key = key;
            this.properties = properties;
        }

        public AnimationKeyFrame(float key, StyleProperty property) {
            this.key = key;
            this.properties = ArrayPool<StyleProperty>.GetExactSize(1);
            this.properties[0] = property;
        }

    }

}