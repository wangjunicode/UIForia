using System.Collections.Generic;
using UIForia.Rendering;

namespace UIForia.Animation {

    public class StyleAnimationData : AnimationData {

        public IList<AnimationKeyFrame2> frames;

        public StyleAnimationData(AnimationOptions options, IList<AnimationKeyFrame2> frames, IList<AnimationTrigger> triggers = null) : base(options) {
            this.frames = frames;
        }

    }

}