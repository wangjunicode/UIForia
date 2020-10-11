using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Sound;
using UIForia.Systems;
using UIForia.Util;

namespace Documentation.Features {

    public class SelectOptionService<T> where T : Enum {

        public static List<ISelectOption<T>> EnumToSelectOptions() {
            List<ISelectOption<T>> result = new List<ISelectOption<T>>();
            T[] values = (T[]) Enum.GetValues(typeof(T));
            for (int i = 0; i < values.Length; i++) {
                result.Add(new SelectOption<T>(values[i].ToString(), values[i]));
            }

            return result;
        }

    }

    [Template("Features/AnimationDemo.xml")]
    public class AnimationDemo : UIElement {

        private UIElement animationTarget;

        public AnimationData animationData;

        public List<ISelectOption<string>> Options;

        public string SelectedOption;

        public float duration;
        public float delay;
        public int iterations;
        public EasingFunction timingFunction;
        public AnimationDirection direction;

        public List<ISelectOption<EasingFunction>> timingFunctions = SelectOptionService<EasingFunction>.EnumToSelectOptions();
        public List<ISelectOption<AnimationDirection>> directions = SelectOptionService<AnimationDirection>.EnumToSelectOptions();

        public AnimationTask animationTask;

        public bool ShowRunButton => (animationTask.state & UITaskState.ReRunnable) != 0;
        public bool ShowPauseButton => (animationTask.state & UITaskState.Pausable) != 0;
        public bool ShowResumeButton => (animationTask.state & UITaskState.Paused) != 0;
        public bool ShowStopButton => (animationTask.state & UITaskState.Stoppable) != 0;

        public override void OnCreate() {
            animationTarget = FindById("animation-target");
            Options = new List<ISelectOption<string>>() {
                new SelectOption<string>("None", "1"),
                new SelectOption<string>("A bit", "2"),
                new SelectOption<string>("Rather more", "3"),
                new SelectOption<string>("Unlimited", "4"),
                new SelectOption<string>("A very very long text label thing here", "5")
            };
        }

        public void ChangeAnimation(string animation) {
            
            if (!animationTarget.Animator.TryGetAnimationData(animation, out animationData)) {
                 return;   
            }

            animationTask = animationTarget.Animator.PlayAnimation(animationData);
            
            if (animationData.options.duration.HasValue) {
                duration = animationData.options.duration.Value.AsMilliseconds;
            }
            else {
                duration = 1000;
            }

            if (animationData.options.delay.HasValue) {
                delay = animationData.options.delay.Value.AsMilliseconds;
            }
            else {
                delay = 0;
            }

            iterations = animationData.options.iterations ?? 1;
            timingFunction = animationData.options.timingFunction ?? EasingFunction.Linear;
            direction = animationData.options.direction ?? AnimationDirection.Forward;
        }

        public void RunAnimationAgain() {
            animationData.options.duration = new UITimeMeasurement(duration);
            animationData.options.delay = new UITimeMeasurement(delay);
            animationData.options.iterations = iterations;
            animationData.options.timingFunction = timingFunction;
            animationData.options.direction = direction;

            animationTask = animationTarget.Animator.PlayAnimation(animationData);
        }

        public void PauseAnimation() {
            animationTarget.Animator.PauseAnimation(animationData);
        }

        public void ResumeAnimation() {
            animationTarget.Animator.ResumeAnimation(animationData);
        }

        public void StopAnimation() {
            animationTarget.Animator.StopAnimation(animationData);
        }

    }

}