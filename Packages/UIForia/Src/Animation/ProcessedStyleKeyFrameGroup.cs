using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Animation {
    
    public struct ProcessedStyleKeyFrameGroup {

        public readonly StylePropertyId propertyId;
        public readonly LightList<ProcessedStyleKeyFrame> frames;

        public ProcessedStyleKeyFrameGroup(StylePropertyId propertyId, LightList<ProcessedStyleKeyFrame> frames) {
            this.propertyId = propertyId;
            this.frames = frames;
        }

    }

}