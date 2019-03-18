using System;
using System.Threading.Tasks;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public enum UITaskResult {

        Running = 1 << 0,
        Completed = 1 << 1,
        Restarted = 1 << 2, // no effect on completion
        Failed = 1 << 3,
        Canceled = Completed | (1 << 4) // like failing but counts as completed

    }

    [Flags]
    public enum UITaskState {

        Uninitialized = 0,
        Pending = 1 << 0,
        Running = 1 << 2,
        Completed = 1 << 3,
        Restarted = 1 << 4,
        Failed = 1 << 5,
        Canceled = 1 << 6

    }

//    [Flags]
//    public enum UITaskState {
//
//        Uninitialized = 0,
//        Pending = 1 << 0,
//        Running = 1 << 2,
//        Completed = 1 << 3,
//        Restarted = 1 << 4,
//        Failed = 1 << 5,
//        Canceled = 1 << 6
//
//    }

    public abstract class UITask {

        public int Id { get; internal set; }
        public string DisplayName { get; set; }
        public float StartTime { get; internal set; }
        public float ElapsedTime { get; internal set; }
        public int ResetCount { get; internal set; }

        public virtual void OnStart() { }
        public virtual void OnComplete() { }
        public virtual void OnReset() { }
        public virtual void OnFailed() { }
        public virtual void OnCancelled() { }
        public virtual void OnInitialized() { }

        public abstract UITaskResult Run(float deltaTime);

    }

    internal struct TaskStatusPair {

        public readonly UITask task;
        public readonly UITaskState state;

        public TaskStatusPair(UITask task, UITaskState state) {
            this.task = task;
            this.state = state;
        }

    }

    internal class CallbackTask : UITask {

        private readonly Func<float, UITaskResult> task;

        public CallbackTask(Func<float, UITaskResult> task) {
            this.task = task;
        }

        public override UITaskResult Run(float deltaTime) {
            return task(deltaTime);
        }

    }

    internal class CallbackTaskNoArg : UITask {

        private readonly Func<UITaskResult> task;

        public CallbackTaskNoArg(Func<UITaskResult> task) {
            this.task = task;
        }

        public override UITaskResult Run(float deltaTime) {
            return task();
        }

    }

    internal class CallbackTaskWithContextNoArg : UITask {

        private readonly Func<UITask, UITaskResult> task;

        public CallbackTaskWithContextNoArg(Func<UITask, UITaskResult> task) {
            this.task = task;
        }

        public override UITaskResult Run(float deltaTime) {
            return task(this);
        }

    }

    internal class CallbackTaskWithContext : UITask {

        private readonly Func<UITask, float, UITaskResult> task;

        public CallbackTaskWithContext(Func<UITask, float, UITaskResult> task) {
            this.task = task;
        }

        public override UITaskResult Run(float deltaTime) {
            return task(this, deltaTime);
        }

    }

    public class UITaskGroup : UITask {

        // run all tasks and complete when all are done or any cancel
        private readonly LightList<TaskStatusPair> taskStatusPairs;

        public UITaskGroup() {
            this.taskStatusPairs = new LightList<TaskStatusPair>(4);
        }

        public void AddTask(UITask task) {
            taskStatusPairs.Add(new TaskStatusPair(task, UITaskState.Uninitialized));
        }

        public void AddTask(Func<float, UITaskResult> task) {
            taskStatusPairs.Add(new TaskStatusPair(new CallbackTask(task), UITaskState.Uninitialized));
        }

        public void AddTask(Func<UITaskResult> task) {
            taskStatusPairs.Add(new TaskStatusPair(new CallbackTaskNoArg(task), UITaskState.Uninitialized));
        }

        public void RemoveTask(UITask task) {
            int index = taskStatusPairs.FindIndex(task, (t, item) => item == t.task);
            if (index != -1) {
                taskStatusPairs.RemoveAt(index);
            }
        }

        public void CancelTask(UITask task) {
            int index = taskStatusPairs.FindIndex(task, (t, item) => item == t.task);
            if (index != -1) {
                task.OnCancelled();
                taskStatusPairs[index] = new TaskStatusPair(task, UITaskState.Canceled);
            }
        }

        public override UITaskResult Run(float deltaTime) {
            TaskStatusPair[] pairs = taskStatusPairs.Array;
            int completedCount = 0;
            int failedCount = 0;

            for (int i = 0; i < taskStatusPairs.Count; i++) {
                TaskStatusPair pair = pairs[i];
                UITaskState state = pair.state;
                UITask task = pair.task;

                if (state == UITaskState.Uninitialized) {
                    state = UITaskState.Pending;
                    task.OnInitialized();
                }

                if ((state & (UITaskState.Pending | UITaskState.Running)) != 0) {
                    UITaskResult result = task.Run(state == UITaskState.Pending ? 0 : deltaTime);
                    switch (result) {
                        case UITaskResult.Running:
                            break;
                        case UITaskResult.Completed:
                            task.OnComplete();
                            break;
                        case UITaskResult.Restarted:
                            break;
                        case UITaskResult.Failed:
                            task.OnFailed();
                            break;
                        case UITaskResult.Canceled:
                            task.OnCancelled();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (state == UITaskState.Completed) {
                    completedCount++;
                }

                if (state == UITaskState.Failed) {
                    break;
                }
            }

            if (failedCount > 0) {
                return UITaskResult.Failed;
            }

            if (completedCount == taskStatusPairs.Count) {
                return UITaskResult.Completed;
            }

            return UITaskResult.Running;
        }

    }

    class UITaskSequence : UITaskGroup {

        // run all tasks seqentially
        // stop when all are done or any cancel

    }

    public struct UITaskData {

        public int id;
        public int restartCount;
        public UITaskState state;
        public float deltaTime;

    }

    // owned by a UIForia Application
    public class UITaskSystem {

        private LightList<TaskStatusPair> taskStatusPairsThisFrame;
        private LightList<TaskStatusPair> taskStatusPairsNextFrame;

        internal void OnReset() { }

        internal void OnInitialize() { }

        internal void OnUpdate() {
            TaskStatusPair[] pairs = taskStatusPairsThisFrame.Array;
            float delta = Time.deltaTime;

            int count = taskStatusPairsThisFrame.Count;
            for (int i = 0; i < count; i++) {
                TaskStatusPair pair = pairs[i];
                UITaskState state = pair.state;
                UITask task = pair.task;

                if (state == UITaskState.Uninitialized) {
                    state = UITaskState.Running;
                    task.OnInitialized();
                }
                else if (state == UITaskState.Pending) {
                    state = task.Run(0);
                }
                else if (state == UITaskState.Running) {
                    task.Run(delta);
                }

                if (state == UITaskState.Completed) {
                    task.OnComplete();
                }

                if (state == UITaskState.Failed) {
                    task.OnFailed();
                }

                if (state == UITaskState.Canceled) {
                    task.OnCancelled();
                }

                if (state == UITaskState.Restarted) {
                    task.OnReset();
                }

                taskStatusPairsNextFrame.Add(new TaskStatusPair(task, UITaskState.Running));
            }

            LightList<TaskStatusPair> swap = taskStatusPairsNextFrame;
            taskStatusPairsNextFrame = taskStatusPairsThisFrame;
            taskStatusPairsThisFrame = swap;
        }

        public void AddTask(UITask task) { }

        public void RemoveTask(UITask task) { }

        public void CancelTask(UITask task) { }

    }

}