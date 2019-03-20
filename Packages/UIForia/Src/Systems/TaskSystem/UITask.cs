using System;

namespace UIForia.Systems {

    public abstract class UITask {

        private const UITaskState k_CanBeEnded = UITaskState.Pending | UITaskState.Restarting | UITaskState.Running;

        internal IUITaskRunner owner;
        internal UITaskState state;

        public float StartTime { get; internal set; }
        public float RestartTime { get; internal set; }
        public float ElapsedTime { get; internal set; }
        public int ResetCount { get; internal set; }
        public int FrameCount { get; internal set; }

        public void Complete() {
            owner?.CompleteTask(this);
        }

        public void Cancel() {
            owner?.CancelTask(this);
        }

        public void Restart() {
            owner?.RestartTask(this);
        }

        public void Fail() {
            owner?.FailTask(this);
        }

        public virtual void OnCompleted() { }
        public virtual void OnRestarted() { }
        public virtual void OnFailed() { }
        public virtual void OnCancelled() { }
        public virtual void OnInitialized() { }

        public abstract UITaskResult Run(float deltaTime);

        public static implicit operator UITask(Func<float, UITaskResult> fn) {
            return new CallbackTask(fn);
        }

        public static implicit operator UITask(Func<UITaskResult> fn) {
            return new CallbackTaskNoArg(fn);
        }

        public static implicit operator UITask(Func<UITask, float, UITaskResult> fn) {
            return new CallbackTaskWithContext(fn);
        }

        public static implicit operator UITask(Func<UITask, UITaskResult> fn) {
            return new CallbackTaskWithContextNoArg(fn);
        }

    }

}