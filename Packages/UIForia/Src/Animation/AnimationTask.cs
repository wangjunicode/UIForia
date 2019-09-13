using System.Collections.Generic;
using UIForia.Systems;

namespace UIForia.Animation {

    public abstract class AnimationTask : UITask {

        public readonly AnimationData animationData;
        public readonly AnimationTaskType type;
        public readonly IList<AnimationTriggerState> triggerStates;
        
        protected AnimationTask(AnimationTaskType type, IList<AnimationTrigger> triggers) {
            this.type = type;
            if (triggers != null) {
                triggerStates = new List<AnimationTriggerState>(triggers.Count);
                for (int i = 0; i < triggers.Count; i++) {
                    triggerStates.Add(new AnimationTriggerState(triggers[i]));
                }
            }
        }

       
        
    }

}