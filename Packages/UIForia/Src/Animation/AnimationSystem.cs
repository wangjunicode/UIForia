using System;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.Remoting;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Animation {

    public class AnimationSystem : ISystem {

        private LightList<AnimationTask> thisFrame;
        private LightList<AnimationTask> nextFrame;

        public AnimationSystem() {
            thisFrame = new LightList<AnimationTask>();
            nextFrame = new LightList<AnimationTask>();
        }

        public AnimationTask Animate(UIElement element, ref AnimationData styleAnimation) {
            AnimationTask task = FindAnimationTask(element, ref styleAnimation, thisFrame);
            if (task != null) {
                task.state = UITaskState.Running;
                return task;
            }
            
            styleAnimation.options = EnsureDefaultOptionValues(styleAnimation);
            switch (styleAnimation.options.playbackType) {
                case AnimationPlaybackType.KeyFrame: {
                    StyleKeyFrameAnimation animationTask = new StyleKeyFrameAnimation(element, styleAnimation);
                    thisFrame.Add(animationTask);
                    return animationTask;
                }

                case AnimationPlaybackType.Parallel:
//                    package.task = taskSystem.AddTask(new ParallelStyleAnimation2());
                    break;
                case AnimationPlaybackType.Sequential:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public void PauseAnimation(UIElement element, ref AnimationData animationData) {
            AnimationTask task = FindAnimationTask(element, ref animationData, thisFrame);
            if (task == null) {
                task = FindAnimationTask(element, ref animationData, nextFrame);
            }

            if (task == null) {
                return;
            }

            if (task is StyleAnimation styleAnimation) {
                styleAnimation.state = UITaskState.Paused;
                task.OnPaused();
            }
        }

        public void ResumeAnimation(UIElement element, ref AnimationData animationData) {
            AnimationTask task = FindAnimationTask(element, ref animationData, thisFrame);
            if (task == null) {
                task = FindAnimationTask(element, ref animationData, nextFrame);
            }

            if (task == null) {
                return;
            }

            if (task is StyleAnimation styleAnimation) {
                styleAnimation.state = UITaskState.Running;
            }
        }

        private AnimationTask FindAnimationTask(UIElement element, ref AnimationData animationData, LightList<AnimationTask> tasks) {
            for (int index = 0; index < tasks.Count; index++) {
                AnimationTask task = tasks[index];
                if (task is StyleAnimation styleAnimation 
                    && styleAnimation.target == element 
                    && task.animationData.name == animationData.name
                    && task.animationData.fileName == animationData.fileName) {

                    return task;
                }
            }

            return null;
        }

        public void StopAnimation(UIElement element, ref AnimationData animationData) {
            AnimationTask task = FindAnimationTask(element, ref animationData, thisFrame);
            if (task == null) {
                task = FindAnimationTask(element, ref animationData, nextFrame);
            }

            if (task == null) {
                return;
            }

            if (task is StyleAnimation styleAnimation) {
                styleAnimation.state = UITaskState.Cancelled;
                task.OnCancelled();
            }
        }

        private static AnimationOptions EnsureDefaultOptionValues(AnimationData data) {
            AnimationOptions options = new AnimationOptions();
            options.duration = data.options.duration ?? 1000;
            options.iterations = data.options.iterations ?? 1;
            options.timingFunction = data.options.timingFunction ?? EasingFunction.Linear;
            options.delay = data.options.delay ?? 0f;
            options.direction = data.options.direction ?? AnimationDirection.Forward;
            options.loopType = data.options.loopType ?? AnimationLoopType.Constant;
            return options;
        }

        public void OnReset() { }

        public void OnUpdate() {
            int count = thisFrame.Count;
            AnimationTask[] animations = thisFrame.Array;

            // todo handle generics in a different function / list set

            for (int i = 0; i < thisFrame.Count; i++) {
                AnimationTask task = thisFrame[i];
                
                StyleAnimation styleAnimation = (StyleAnimation) task;

                if (styleAnimation.state == UITaskState.Paused) {
                    nextFrame.Add(styleAnimation);
                    continue;
                }

                if (styleAnimation.target.isDestroyed) {
                    styleAnimation.state = UITaskState.Failed;
                    continue;
                }

                if (styleAnimation.target.isDisabled) {
                    // if stop on disable
                    task.OnPaused();
                    // task.OnCancel?(); handle restoring to start state?
                    styleAnimation.state = UITaskState.Paused;
                    continue;
                }

                AnimationData styleAnimationAnimationData = styleAnimation.animationData;
                if (styleAnimation.state == UITaskState.Uninitialized) {
                    styleAnimation.state = UITaskState.Running;
                    styleAnimationAnimationData.onStart?.Invoke(new StyleAnimationEvent(
                        AnimationEventType.Start,
                        styleAnimation.target,
                        styleAnimation.status,
                        styleAnimationAnimationData.options)
                    );
                }

                UITaskResult status = styleAnimation.Run(Time.unscaledDeltaTime);

                switch (status) {
                    case UITaskResult.Running:
                        styleAnimation.RunTriggers();

                        styleAnimationAnimationData.onTick?.Invoke(new StyleAnimationEvent(
                            AnimationEventType.Tick,
                            styleAnimation.target,
                            styleAnimation.status,
                            styleAnimationAnimationData.options)
                        );

                        nextFrame.Add(styleAnimation);
                        break;

                    case UITaskResult.Completed:
                        styleAnimation.RunTriggers();
                        styleAnimation.status.currentIteration++;
                        styleAnimation.status.elapsedIterationTime = 0f;
                        styleAnimation.state = UITaskState.Completed;

                        if (styleAnimation.status.currentIteration == styleAnimationAnimationData.options.iterations) {
                            styleAnimationAnimationData.onCompleted?.Invoke(new StyleAnimationEvent(
                                AnimationEventType.Complete,
                                styleAnimation.target,
                                styleAnimation.status,
                                styleAnimationAnimationData.options)
                            );
                            styleAnimationAnimationData.onEnd?.Invoke(new StyleAnimationEvent(
                                AnimationEventType.End,
                                styleAnimation.target,
                                styleAnimation.status,
                                styleAnimationAnimationData.options)
                            );
                            continue;
                        }

                        styleAnimation.state = UITaskState.Pending;

                        styleAnimation.ResetTriggers();
                        if (styleAnimationAnimationData.options.loopType == AnimationLoopType.PingPong) {
                            if (styleAnimationAnimationData.options.direction == AnimationDirection.Reverse) {
                                styleAnimationAnimationData.options.direction = AnimationDirection.Forward;
                            }
                            else {
                                styleAnimationAnimationData.options.direction = AnimationDirection.Reverse;
                            }
                        }

                        nextFrame.Add(styleAnimation);
                        break;

                    case UITaskResult.Restarted:
                        // todo -- if direction != oldDirection -> invoke direction change
                        nextFrame.Add(styleAnimation);
                        break;

                    case UITaskResult.Failed:
                    case UITaskResult.Cancelled:
                        styleAnimation.state = UITaskState.Failed;
                        styleAnimationAnimationData.onCanceled?.Invoke(new StyleAnimationEvent(
                            AnimationEventType.Cancel,
                            styleAnimation.target,
                            styleAnimation.status,
                            styleAnimationAnimationData.options)
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            LightList<AnimationTask> swap = nextFrame;
            nextFrame = thisFrame;
            thisFrame = swap;
            nextFrame.QuickClear();
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnElementCreated(UIElement element) { }

        public void OnElementEnabled(UIElement element) {
            // restore animations as needed
        }

        public void OnElementDisabled(UIElement element) {
//            animatedElements.FindIndex(element);
            // traverse children
            // find any with active animations
            // pause animations as needed
        }

        public void OnElementDestroyed(UIElement element) {
            // animatedElements.FindIndex(element);
            // traverse children
            // find any with active animations
            // stop animations as needed, no callbacks
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

    }

}