using UIForia.Systems;

namespace UIForia.Animation {

    public abstract class AnimationTask : UITask {

        public readonly AnimationTaskType type;
        
        protected AnimationTask(AnimationTaskType type) {
            this.type = type;
        }
        
    }

}