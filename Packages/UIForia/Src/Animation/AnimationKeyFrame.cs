using System.Collections.Generic;
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
    
    public struct AnimationKeyFrame2 {

        public readonly float key;
        public LightList<StyleKeyFrameValue> properties;

        public AnimationKeyFrame2(float key, StyleKeyFrameValue p0) {
            this.key = key;
            this.properties = LightListPool<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(1);
            this.properties[0] = p0;
            this.properties.Count = 1;
        }

        public AnimationKeyFrame2(float key, StyleKeyFrameValue p0, StyleKeyFrameValue p1) {
            this.key = key;
            this.properties = LightListPool<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(2);
            this.properties[0] = p0;
            this.properties[1] = p1;
            this.properties.Count = 2;
        }
        
        public AnimationKeyFrame2(float key, StyleKeyFrameValue p0, StyleKeyFrameValue p1, StyleKeyFrameValue p2) {
            this.key = key;
            this.properties = LightListPool<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(3);
            this.properties[0] = p0;
            this.properties[1] = p1;
            this.properties[2] = p2;
            this.properties.Count = 3;
        }
        
        public AnimationKeyFrame2(float key, StyleKeyFrameValue p0, StyleKeyFrameValue p1, StyleKeyFrameValue p2, StyleKeyFrameValue p3) {
            this.key = key;
            this.properties = LightListPool<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(4);
            this.properties[0] = p0;
            this.properties[1] = p1;
            this.properties[2] = p2;
            this.properties[3] = p3;
            this.properties.Count = 4;
        }
        
        public AnimationKeyFrame2(float key, params StyleKeyFrameValue[] properties) {
            this.key = key;
            this.properties = LightListPool<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(properties.Length);
            for (int i = 0; i < properties.Length; i++) {
                this.properties[i] = properties[i];
            }

            this.properties.Count = properties.Length;
        }
        
        public AnimationKeyFrame2(float key, IList<StyleKeyFrameValue> properties) {
            this.key = key;
            this.properties = LightListPool<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(properties.Count);
            for (int i = 0; i < properties.Count; i++) {
                this.properties[i] = properties[i];
            }
            this.properties.Count = properties.Count;
        }

        public void Release() {
            LightListPool<StyleKeyFrameValue>.Release(ref properties);
        }
        
    }

}