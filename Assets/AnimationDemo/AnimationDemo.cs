using SVGX;
using UIForia;
using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace Demo {

    [Template("AnimationDemo/AnimationDemo.xml")]
    public class AnimationDemo : UIElement {

        private UIElement one_Blue;
        private UIElement one_BlueOrbit;
        private UIElement two_YellowBg;
        private UIElement two_RedBg;
        private UIElement bouncer;

        private static Color32 c1 = new Color32(0x15, 0x65, 0xc0, 0xff);
        private static Color32 c2 = new Color32(0xb7, 0x1c, 0x1c, 0xff);
        private static Color32 c3 = new Color32(0xff, 0xeb, 0x3b, 0xff);
        private static Color32 c4 = new Color32(0x2e, 0x7d, 0x32, 0xff);
        private static Color32 c5 = new Color32(0x00, 0xbc, 0xd4, 0xff);
        private static Color32 c6 = new Color32(0xef, 0x6c, 0x00, 0xff);

        public override void OnCreate() {
            one_Blue = FindById("one_blue");
            one_BlueOrbit = FindById("one_blueOrbit");
            two_YellowBg = FindById("two_yellow-bg");
            two_RedBg = FindById("two_red-bg");
            bouncer = FindById("bouncer");
            
            Application.Animate(one_Blue, ScaleAnim());
            Application.Animate(two_RedBg, RedBgAnim());
            Application.Animate(two_YellowBg, YellowBgAnim());
            
        }

        public StyleAnimationData RedBgAnim() {
            AnimationOptions options = new AnimationOptions() {
                duration = 3200,
                iterations = AnimationOptions.InfiniteIterations,
            };

            AnimationKeyFrame2[] frames = {
                new AnimationKeyFrame2(0f,
                    StyleProperty.BorderColor(c2),
                    StyleProperty.BorderTop(0),
                    StyleProperty.BorderRight(0),
                    StyleProperty.BorderBottom(0),
                    StyleProperty.BorderLeft(0)
                ),
                new AnimationKeyFrame2(0.05f,
                    StyleProperty.TransformScaleX(0.6f),
                    StyleProperty.TransformScaleY(0.6f),
                    StyleProperty.BackgroundColor(c2)
                ),
                new AnimationKeyFrame2(0.2f,
                    StyleProperty.TransformScaleX(0.6f),
                    StyleProperty.TransformScaleY(0.6f),
                    StyleProperty.BackgroundColor(Color.clear)
                ),
                new AnimationKeyFrame2(0.35f,
                    StyleProperty.BackgroundColor(c2),
                    StyleProperty.BorderTop(12),
                    StyleProperty.BorderRight(12),
                    StyleProperty.BorderBottom(12),
                    StyleProperty.BorderLeft(12)
                ),
                new AnimationKeyFrame2(0.5f,
                    StyleProperty.BackgroundColor(c2),
                    StyleProperty.BorderTop(6),
                    StyleProperty.BorderRight(6),
                    StyleProperty.BorderBottom(6),
                    StyleProperty.BorderLeft(6)
                ),
                new AnimationKeyFrame2(0.8f,
                    StyleProperty.BackgroundColor(c2),
                    StyleProperty.BorderTop(6),
                    StyleProperty.BorderRight(6),
                    StyleProperty.BorderBottom(6),
                    StyleProperty.BorderLeft(6),
                    StyleProperty.TransformScaleX(0.6f),
                    StyleProperty.TransformScaleY(0.6f)
                ),
                new AnimationKeyFrame2(0.95f,
                    StyleProperty.BackgroundColor(c2),
                    StyleProperty.BorderTop(0),
                    StyleProperty.BorderRight(0),
                    StyleProperty.BorderBottom(0),
                    StyleProperty.BorderLeft(0),
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f)
                ),
                new AnimationKeyFrame2(1f,
                    StyleProperty.BorderTop(0),
                    StyleProperty.BorderRight(0),
                    StyleProperty.BorderBottom(0),
                    StyleProperty.BorderLeft(0),
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f)
                ),
            };

            return new StyleAnimationData(options, frames);
        }

        public StyleAnimationData OrbitFadeAnim() {

            AnimationOptions options = new AnimationOptions() {
                duration = 1000,
                iterations = 1,
            };

            AnimationKeyFrame2[] frames = {
                new AnimationKeyFrame2(0f,
                    new StyleKeyFrameValue(StylePropertyId.TransformRotation, "$startVal")
                ),
                new AnimationKeyFrame2(1f,
                    new StyleKeyFrameValue(StylePropertyId.TransformRotation, "$startVal + 540f")
                )
            };

            return new StyleAnimationData(options, frames);
        }

        public StyleAnimationData YellowBgAnim() {
            AnimationOptions options = new AnimationOptions() {
                duration = 3200,
                iterations = AnimationOptions.InfiniteIterations,
            };

            AnimationKeyFrame2[] frames = {
                new AnimationKeyFrame2(0.45f,
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f)
                ),
                new AnimationKeyFrame2(0.55f,
                    StyleProperty.TransformScaleX(1.18f),
                    StyleProperty.TransformScaleY(1.18f)
                ),
                new AnimationKeyFrame2(0.80f,
                    StyleProperty.TransformScaleX(1.18f),
                    StyleProperty.TransformScaleY(1.18f)
                ),
                new AnimationKeyFrame2(0.90f,
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f)
                ),
                new AnimationKeyFrame2(1f,
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f)
                )
            };

            return new StyleAnimationData(options, frames);
        }

        public StyleAnimationData ScaleAnim() {
            AnimationOptions options = new AnimationOptions() {
                duration = 3200,
                iterations = AnimationOptions.InfiniteIterations,
            };

            AnimationKeyFrame2[] frames = {
                new AnimationKeyFrame2(0,
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f),
                    StyleProperty.BackgroundColor(c1)
                ),
                new AnimationKeyFrame2(0.05f,
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f),
                    StyleProperty.BackgroundColor(c1)
                ),
                new AnimationKeyFrame2(0.1f,
                    StyleProperty.BackgroundColor(c3)
                ),
                new AnimationKeyFrame2(0.15f,
                    StyleProperty.TransformScaleX(0.4f),
                    StyleProperty.TransformScaleY(0.4f)
                ),
                new AnimationKeyFrame2(0.5f,
                    StyleProperty.TransformScaleX(0.4f),
                    StyleProperty.TransformScaleY(0.4f),
                    StyleProperty.BackgroundColor(c3)
                ),
                new AnimationKeyFrame2(0.55f,
                    StyleProperty.BackgroundColor(c1)
                ),
                new AnimationKeyFrame2(0.65f,
                    StyleProperty.TransformScaleX(0.8f),
                    StyleProperty.TransformScaleY(0.8f)
                ),
                new AnimationKeyFrame2(0.8f,
                    StyleProperty.TransformScaleX(0.8f),
                    StyleProperty.TransformScaleY(0.8f)
                ),
                new AnimationKeyFrame2(0.95f,
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f)
                ),
                new AnimationKeyFrame2(1f,
                    StyleProperty.TransformScaleX(1f),
                    StyleProperty.TransformScaleY(1f)
                ),
            };

            AnimationTrigger[] triggers = {
                new AnimationTrigger(0.65f, () => {
                    one_BlueOrbit.SetEnabled(true);
                    Application.Animate(one_BlueOrbit, OrbitFadeAnim());
                }),
                new AnimationTrigger(),
                new AnimationTrigger(),
            };

            return new StyleAnimationData(options, frames, triggers);
        }

    }

}