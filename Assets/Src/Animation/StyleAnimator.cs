using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Animation {

    public enum AnimationDirection {

        Forward,
        Reverse,

    }

    public enum AnimationLoopType {

        Forward,
        Reverse,
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

        protected List<StyleAnimation> m_PlayingAnimations;
        protected List<AnimDef> m_QueuedAnimations;
        protected Rect m_Viewport;

        protected static int NextId;
        
        public StyleAnimator() {
            this.m_PlayingAnimations = new List<StyleAnimation>();
            this.m_QueuedAnimations = new List<AnimDef>();
            AnimationKeyFrame[] a = {
                new AnimationKeyFrame(0f,
                    StyleProperty.TransformPositionX(100f),
                    StyleProperty.TransformPositionY(100f)
                ),
                new AnimationKeyFrame(0.1f,
                    StyleProperty.PreferredWidth(200)
                ),
            };
        }
        
        public void SetViewportRect(Rect viewport) {
            m_Viewport = viewport;
        }

        public int PlayAnimation( UIStyleSet styleSet, StyleAnimation animation, AnimationOptions options) {
            m_QueuedAnimations.Add(new AnimDef(styleSet, animation, options));
            return NextId++;
        }

        public void OnUpdate() {
            for (int i = 0; i < m_QueuedAnimations.Count; i++) {
                AnimDef anim = m_QueuedAnimations[i];

                AnimationOptions baseOptions = anim.animation.options;
                AnimationOptions overrideOptions = anim.options;
                
                anim.styleSet.OnAnimationStart();
                
            }

            float deltaTime = Time.deltaTime;
            for (int i = 0; i < m_PlayingAnimations.Count; i++) {
                StyleAnimation anim = m_PlayingAnimations[i];
                anim.Update(anim., m_Viewport, deltaTime);
            }
        }

        public void PauseAnimation(int animationId) { }

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