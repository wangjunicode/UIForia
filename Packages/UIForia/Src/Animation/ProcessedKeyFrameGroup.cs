using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Animation {

    public struct ProcessedKeyFrameGroup {

        public readonly StylePropertyId propertyId;
        public readonly LightList<ProcessedKeyFrame> frames;

        public ProcessedKeyFrameGroup(StylePropertyId propertyId, LightList<ProcessedKeyFrame> frames) {
            this.propertyId = propertyId;
            this.frames = frames;
        }

    }

}