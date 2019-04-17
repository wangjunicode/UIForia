using System;
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

        public void Animate(UIElement element, AnimationData styleAnimation) {
            styleAnimation.options = EnsureDefaultOptionValues(styleAnimation);
            switch (styleAnimation.options.playbackType) {
                case AnimationPlaybackType.KeyFrame: {
                    thisFrame.Add(new StyleKeyFrameAnimation(element, styleAnimation));
                    break;
                }

                case AnimationPlaybackType.Parallel:
//                    package.task = taskSystem.AddTask(new ParallelStyleAnimation2());
                    break;
                case AnimationPlaybackType.Sequential:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

                if (styleAnimation.target.isDestroyed) {
                    styleAnimation.state = UITaskState.Failed;
                    continue;
                }
                else if (styleAnimation.target.isDisabled) {
                    // if stop on disable
                    // task.OnPaused();
                    // task.OnCancel?(); handle restoring to start state?
                    // styleAnimation.state = UITaskState.Pending;
                    //continue;
                }

                if (styleAnimation.state == UITaskState.Uninitialized) {
                    styleAnimation.data.onStart?.Invoke(new StyleAnimationEvent(
                        AnimationEventType.Start,
                        styleAnimation.target,
                        styleAnimation.status,
                        styleAnimation.data.options)
                    );
                }
                
                UITaskResult status = styleAnimation.Run(Time.deltaTime);

                switch (status) {
                    case UITaskResult.Running:
                        styleAnimation.RunTriggers();

                        styleAnimation.data.onTick?.Invoke(new StyleAnimationEvent(
                            AnimationEventType.Tick,
                            styleAnimation.target,
                            styleAnimation.status,
                            styleAnimation.data.options)
                        );

                        nextFrame.Add(styleAnimation);
                        break;

                    case UITaskResult.Completed:
                        styleAnimation.RunTriggers();
                        styleAnimation.status.currentIteration++;
                        styleAnimation.status.elapsedIterationTime = 0f;
                        styleAnimation.state = UITaskState.Completed;

                        if (styleAnimation.status.currentIteration == styleAnimation.data.options.iterations) {
                            styleAnimation.data.onCompleted?.Invoke(new StyleAnimationEvent(
                                AnimationEventType.Complete,
                                styleAnimation.target,
                                styleAnimation.status,
                                styleAnimation.data.options)
                            );
                            styleAnimation.data.onEnd?.Invoke(new StyleAnimationEvent(
                                AnimationEventType.End,
                                styleAnimation.target,
                                styleAnimation.status,
                                styleAnimation.data.options)
                            );
                            continue;
                        }
                        
                        styleAnimation.state = UITaskState.Pending;

                        styleAnimation.ResetTriggers();
                        // todo -- if direction != oldDirection -> invoke direction change
                        nextFrame.Add(styleAnimation);
                        break;

                    case UITaskResult.Restarted:
                        // todo -- if direction != oldDirection -> invoke direction change
                        nextFrame.Add(styleAnimation);
                        break;

                    case UITaskResult.Failed:
                    case UITaskResult.Cancelled:
                        styleAnimation.state = UITaskState.Failed;
                        styleAnimation.data.onCanceled?.Invoke(new StyleAnimationEvent(
                            AnimationEventType.Cancel,
                            styleAnimation.target,
                            styleAnimation.status,
                            styleAnimation.data.options)
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