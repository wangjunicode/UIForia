using System;
using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;

namespace Documentation.Features {

    public class SelectOptionService<T> where T : Enum {
        
        public static RepeatableList<ISelectOption<T>> EnumToSelectOptions() {
            RepeatableList<ISelectOption<T>> result = new RepeatableList<ISelectOption<T>>();
            T[] values = (T[]) Enum.GetValues(typeof(T));
            for (int i = 0; i < values.Length; i++) {
                result.Add(new SelectOption<T>(values[i].ToString(), values[i]));
            }
            return result;
        }
    }

    [Template("Documentation/Features/AnimationDemo.xml")]
    public class AnimationDemo : UIElement {

        private UIElement animationTarget;

        public AnimationData animationData;

        public int duration;
        public float delay;
        public int iterations;
        public EasingFunction timingFunction;
        public AnimationDirection direction;

        public RepeatableList<ISelectOption<EasingFunction>> timingFunctions = SelectOptionService<EasingFunction>.EnumToSelectOptions();
        public RepeatableList<ISelectOption<AnimationDirection>> directions = SelectOptionService<AnimationDirection>.EnumToSelectOptions();

        public AnimationTask animationTask;

        public bool ShowRunButton => (animationTask.state & UITaskState.ReRunnable) != 0;
        public bool ShowPauseButton => (animationTask.state & UITaskState.Pausable) != 0;
        public bool ShowResumeButton => (animationTask.state & UITaskState.Paused) != 0;
        public bool ShowStopButton => (animationTask.state & UITaskState.Stoppable) != 0;

        public override void OnCreate() {
            animationTarget = FindById("animation-target");
        }

        public void ChangeAnimation(string animation) {
            animationData = Application.GetAnimationFromFile("Documentation/Features/AnimationDemo.style", animation);
            animationTask = Application.Animate(animationTarget, animationData);
            duration = animationData.options.duration ?? 1000;
            delay = animationData.options.delay ?? 0f;
            iterations = animationData.options.iterations ?? 1;
            timingFunction = animationData.options.timingFunction ?? EasingFunction.Linear;
            direction = animationData.options.direction ?? AnimationDirection.Forward;
        }

        public void RunAnimationAgain() {
            animationData.options.duration = duration;
            animationData.options.delay = delay;
            animationData.options.iterations = iterations;
            animationData.options.timingFunction = timingFunction;
            animationData.options.direction = direction;
            
            animationTask = Application.Animate(animationTarget, animationData);
        }

        public void PauseAnimation() {
            Application.PauseAnimation(animationTarget, animationData);   
        }

        public void ResumeAnimation() {
            Application.ResumeAnimation(animationTarget, animationData);
        }

        public void StopAnimation() {
            Application.StopAnimation(animationTarget, animationData);
        }
    }

}