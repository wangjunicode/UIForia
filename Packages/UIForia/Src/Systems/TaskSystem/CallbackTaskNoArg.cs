using System;

namespace UIForia.Systems {

    internal class CallbackTaskNoArg : UITask {

        private readonly Func<UITaskResult> task;

        public CallbackTaskNoArg(Func<UITaskResult> task) {
            this.task = task;
        }

        public override UITaskResult Run(float deltaTime) {
            return task();
        }

    }

}