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

        public void Animate(GenericAnimationData animationData) {
//            AnimationTask task = null;
//            switch (animationData.options.playbackType) {
//                case AnimationPlaybackType.KeyFrame:
//                    task = new GenericKeyFrameAnimation(animationData);
//                    thisFrame.Add(task);
//                    break;
//                case AnimationPlaybackType.Parallel:
//                    break;
//                case AnimationPlaybackType.Sequential:
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
        }

        public void Animate(UIElement element, StyleAnimationData styleAnimation) {
            styleAnimation.options = EnsureDefaultOptionValues(styleAnimation);
            switch (styleAnimation.options.playbackType) {
                case AnimationPlaybackType.KeyFrame: {
                    StyleKeyFrameAnimation task = new StyleKeyFrameAnimation(element, styleAnimation);
                    task.SetVariable("target", element);
                    task.ProcessKeyFrames(styleAnimation.frames);
                    thisFrame.Add(task);
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

            for (int i = 0; i < count; i++) {
                AnimationTask task = animations[i];

                StyleAnimation2 styleAnimation = (StyleAnimation2) task;

                if (styleAnimation.target.isDestroyed) {
                    task.state = UITaskState.Failed;
                    continue;
                }
                else if (styleAnimation.target.isDisabled) {
                    // if stop on disable
                    // task.OnPaused();
                    // task.OnCancel?(); handle restoring to start state?
                    task.state = UITaskState.Pending;
                    continue;
                }

                UITaskResult status = styleAnimation.Run(Time.deltaTime);

                switch (status) {
                    case UITaskResult.Running:
                        styleAnimation.data.onTick?.Invoke(styleAnimation.status);
                        nextFrame.Add(styleAnimation);
                        break;
                    case UITaskResult.Completed:
                        styleAnimation.status.currentIteration++;
                        styleAnimation.status.elapsedIterationTime = 0f;
                        if (styleAnimation.status.currentIteration == styleAnimation.data.options.iterations) {
                            styleAnimation.data.onCompleted?.Invoke(styleAnimation.status);
                            continue;
                        }

                        // todo -- if direction != oldDirection -> invoke direction change
                        nextFrame.Add(styleAnimation);
                        break;
                    case UITaskResult.Restarted:
                        // todo -- if direction != oldDirection -> invoke direction change
                        nextFrame.Add(styleAnimation);
                        break;
                    case UITaskResult.Failed:
                    case UITaskResult.Cancelled:
                        styleAnimation.data.onCanceled?.Invoke(styleAnimation.status);
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