using UIForia.Style;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    internal struct CompiledAnimation {

        public RangeInt animationName;

        public AnimationOptions options;

        // grouped by property id first, then sorted by time
        public StructList<CompiledPropertyKeyFrames> propertyKeyFrames;

    }

    // property keyframe
    internal struct CompiledPropertyKeyFrames {

        public PropertyKeyInfo key;
        public RangeInt valueRange; // styledb -> index into typed table
        public UITimeMeasurement time;

    }

}