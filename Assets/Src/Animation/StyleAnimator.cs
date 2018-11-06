using System.Collections.Generic;
using Src.Rendering;
using Src.Extensions;
using UnityEngine;

namespace Src.Animation {

    public enum AnimationDirection {

        Forward,
        Reverse,

    }

    public enum AnimationLoopType {

        Constant,
        PingPong

    }

    public struct AnimationProgress {

        public float elapsedTime;
        public int iterationCount;
        public AnimationDirection currentDirection;

    }

    public class StyleAnimator {

        protected struct AnimDef {

            public readonly UIStyleSet styleSet;
            public readonly StyleAnimation animation;
            public readonly AnimationOptions options;

            public AnimDef(UIStyleSet styleSet, StyleAnimation animation, AnimationOptions options) {
                this.styleSet = styleSet;
                this.animation = animation;
                this.options = options;
            }

        }

        protected List<AnimDef> m_PlayingAnimations;
        protected List<AnimDef> m_QueuedAnimations;
        protected Rect m_Viewport;

        protected static int NextId;

        public StyleAnimator() {
            this.m_PlayingAnimations = new List<AnimDef>();
            this.m_QueuedAnimations = new List<AnimDef>();
        }

        public void SetViewportRect(Rect viewport) {
            m_Viewport = viewport;
        }

        public int PlayAnimation(UIStyleSet styleSet, StyleAnimation animation, AnimationOptions options = default(AnimationOptions)) {
            m_QueuedAnimations.Add(new AnimDef(styleSet, animation, options));
            return NextId++;
        }

        public void OnUpdate() {
            for (int i = 0; i < m_QueuedAnimations.Count; i++) {
                AnimDef anim = m_QueuedAnimations[i];
                AnimationOptions baseOptions = anim.animation.m_Options;
                AnimationOptions overrideOptions = anim.options;

                if (anim.options == default(AnimationOptions)) { }

                anim.animation.OnStart(anim.styleSet, m_Viewport);
                m_PlayingAnimations.Add(anim);
            }

            m_QueuedAnimations.Clear();

            float deltaTime = Time.deltaTime;
            for (int i = 0; i < m_PlayingAnimations.Count; i++) {
                StyleAnimation anim = m_PlayingAnimations[i].animation;

                if (anim.Update(m_PlayingAnimations[i].styleSet, m_Viewport, deltaTime) == StyleAnimation.AnimationStatus.Completed) {
                    m_PlayingAnimations.UnstableRemove(i);
                    anim.OnComplete();
                }
            }
        }

        public void PauseAnimation(int animationId) { }

        public void Reset() {
            m_PlayingAnimations.Clear();
            m_QueuedAnimations.Clear();
        }

    }

    /*
     *
     * duration
     * delay
     * iteration count
     * curve
     * direction
     *
     * new AnimationSequence(options, targets{}, new Animation[] {
     *    new KeyFrameAnimation(options, frames)),
     *    new PropertyAnimation(options, TransformPositionX),
     *    new AnimationSequence(options, new Animation[] {
     *         new PropertyAnimation(options, "targetName"),
     *         new PropertyAnimation(options, "targetName"),
     *    }
     * });
     * 
     * StyleAnimation.CreateAnimation("name", new AnimationSequence[] {
     *    new AnimationSequence(sequenceTimeSlice, new Animation[] {
     *        new Animation(StyleProperty.TransformX, startValue | InheritStart, UIFixedMeasurement end, Curve curve, delay, duration, pingPong, loopCount)
     *        new Animation(StyleProperty.TransformY, InheritStart, UIFixedMeasurement end, Curve curve, duration, pingPong, loopCount)
     *        new Animation(StyleProperty.PreferredWidth, InheritStart, UIMeasurement end, Curve curve, duration, pingPong, loopCount)
     *        new Animation(StyleProperty.TransformX, InheritStart, UIMeasurement end, Curve curve, duration, pingPong, loopCount)
     *     })
     * });
     * new KeyFrameAnimationGroup(float t, new StyleProperty[] { new StyleProperty() });
     *
     * style.CreateKeyFrameAnimation("name", new AnimationKey(0.2f, new AnimationFrame(preferredWidth, targetValue));
     *
     * animation types
     *
     *     key frame
     *     sequence
     *     controlled
     * 
     * 
     * style.PlayAnimation(animation);
     * style.PlayAnimation(new PropertyAnimation(), options);
     * style.PlayAnimation(animationSequence, optionOverrides);
     * style.Animate(property, target, curve, options)
     * style.PlayAnimation("name");
     * style.StopAnimating();
     * style.PlayAnimation("");
     * style.OnEnterState(StyleState.Hover, () => style.PlayAnimation());
     *
     * style.OnAnimationUpdate(animation);
     * style.OnAnimationStart();
     * style.OnAnimationPause();
     * style.OnAnimationResume();
     * style.OnAnimationEnd();
     * style.OnAnimationComplete();
     * style.OnAnimationCancel();
     * 
     * float p = animation.Progress;
     * style.IsAnimatingProperty(property);
     * style.
     * 
     */

}