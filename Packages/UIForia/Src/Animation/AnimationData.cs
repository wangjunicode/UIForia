using System;
using UIForia.Util;

namespace UIForia.Animation {

    public abstract class AnimationData {

        public AnimationOptions options;
        public LightList<Action> triggers;
        public LightList<AnimationVariable> variables;  
        
        public Action<AnimationState2> onStart;
        public Action<AnimationState2> onEnd;
        public Action<AnimationState2> onCanceled;
        public Action<AnimationState2> onCompleted;
        public Action<AnimationState2> onTick;

        protected AnimationData(AnimationOptions options) {
            this.options = options;
        }
        
        public void SetVariable(string variableName, Type type, object value) {
            
        }

        public void SetVariable<T>(string variableName, T value) {
            
        }
        
    }

}