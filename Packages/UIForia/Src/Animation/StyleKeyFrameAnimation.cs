using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Animation {

   

    public class StyleKeyFrameAnimation : StyleAnimation {

        private readonly LightList<ProcessedKeyFrameGroup> processedFrameGroups;

        public StyleKeyFrameAnimation(UIElement target, AnimationData data) : base(target, data) {
            processedFrameGroups = new LightList<ProcessedKeyFrameGroup>();
            ProcessKeyFrames(data.frames);
        }

        public void ProcessKeyFrames(IList<AnimationKeyFrame> frames) {
            // todo -- ensure we release lists where we need to
            // todo -- dont use a list each processed frame group, use a single list sorted sensibly
            processedFrameGroups.QuickClear();
            for (int i = 0; i < frames.Count; i++) {
                AnimationKeyFrame f = frames[i];
                StyleKeyFrameValue[] properties = f.properties.Array;
                for (int j = 0; j < f.properties.Count; j++) {
                    AddKeyFrame(f.key, properties[j]);
                }
            }

            for (int i = 0; i < processedFrameGroups.Count; i++) {
                StylePropertyId property = processedFrameGroups[i].propertyId;
                LightList<ProcessedKeyFrame> processedKeyFrames = processedFrameGroups[i].frames;
                processedKeyFrames.Sort(KeyFrameSorter);
                if (processedKeyFrames[0].time != 0f) {
                    processedKeyFrames.Insert(0, new ProcessedKeyFrame(0f, target.style.GetComputedStyleProperty(property)));
                }
            }
        }

        private void AddKeyFrame(float time, StyleKeyFrameValue property) {

            for (int i = 0; i < processedFrameGroups.Count; i++) {
                if (processedFrameGroups[i].propertyId == property.propertyId) {
                    processedFrameGroups[i].frames.Add(new ProcessedKeyFrame(time, property));
                    return;
                }
            }

            LightList<ProcessedKeyFrame> list = LightList<ProcessedKeyFrame>.Get();
            list.Add(new ProcessedKeyFrame(time, property));
            processedFrameGroups.Add(new ProcessedKeyFrameGroup(property.propertyId, list));
        }

        public override UITaskResult Run(float deltaTime) {
            status.elapsedTotalTime += deltaTime;
            status.elapsedIterationTime += deltaTime;

            if (state == UITaskState.Cancelled) {
                return UITaskResult.Cancelled;
            }
            
            AnimationOptions options = animationData.options;
            float delay = options.delay?.AsSeconds ?? 0; 
            if (delay > status.elapsedTotalTime) {
                return UITaskResult.Running;
            }
            
            float duration = options.duration?.AsSeconds ?? 1;
            float elapsedIterationTime = status.elapsedIterationTime - delay;
            float progress = Mathf.Clamp01(elapsedIterationTime / duration);

            status.iterationProgress = progress;
            status.frameCount++;

            float targetProgress = progress;
            bool isReversed = options.direction.HasValue && options.direction.Value == AnimationDirection.Reverse;

            if (isReversed) {
                targetProgress = 1 - targetProgress;
            }

            Rect viewport = target.View.Viewport;
            ProcessedKeyFrameGroup[] groups = processedFrameGroups.array;
            ProcessedKeyFrame next = default;
            for (int i = 0; i < processedFrameGroups.Count; i++) {
                StylePropertyId propertyId = groups[i].propertyId;
                ProcessedKeyFrame[] frames = groups[i].frames.array;
                int frameCount = groups[i].frames.size;

                ProcessedKeyFrame prev = frames[0];
                next = frames[frameCount - 1];

                for (int j = 1; j < frameCount; j++) {
                    if (frames[j].time > targetProgress) {
                        prev = frames[j - 1];
                        next = frames[j];
                        break;
                    }
                }

                if (isReversed) {
                    ProcessedKeyFrame tmp = prev;
                    prev = next;
                    next = tmp;
                }

                float t = Mathf.Clamp01(Easing.Interpolate(MathUtil.PercentOfRange(targetProgress, prev.time, next.time), options.timingFunction.Value));

                switch (propertyId) {
                    case StylePropertyId.TransformPivotX:
                    case StylePropertyId.PaddingLeft:
                    case StylePropertyId.PaddingRight:
                    case StylePropertyId.BorderLeft:
                    case StylePropertyId.BorderRight:
                    case StylePropertyId.MarginLeft:
                    case StylePropertyId.MarginRight: {
                        float v0 = ResolveFixedWidth(target, viewport, prev.value.styleProperty.AsUIFixedLength);

                        float v1 = ResolveFixedWidth(target, viewport, next.value.styleProperty.AsUIFixedLength);
                        target.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(Mathf.Lerp(v0, v1, t))));
                        break;
                    }

                    case StylePropertyId.TransformPivotY:
                    case StylePropertyId.PaddingTop:
                    case StylePropertyId.PaddingBottom:
                    case StylePropertyId.BorderTop:
                    case StylePropertyId.BorderBottom:
                    case StylePropertyId.MarginTop:
                    case StylePropertyId.TextFontSize:
                    case StylePropertyId.MarginBottom: {
                        float v0 = ResolveFixedHeight(target, viewport, prev.value.styleProperty.AsUIFixedLength);

                        float v1 = ResolveFixedHeight(target, viewport, next.value.styleProperty.AsUIFixedLength);
                        target.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(Mathf.Lerp(v0, v1, t))));
                        break;
                    }

                    case StylePropertyId.PreferredWidth:
                    case StylePropertyId.MinWidth:
                    case StylePropertyId.MaxWidth: {
                        
                        float v0 = ResolveWidthMeasurement(target, viewport, prev.value.styleProperty.AsUIMeasurement);

                        float v1 = ResolveWidthMeasurement(target, viewport, next.value.styleProperty.AsUIMeasurement);

                        //target.style.SetAnimatedMeasurementProperty(propertyId, prev.value.styleProperty, next.value.styleProperty, t);
                         target.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(Mathf.Lerp(v0, v1, t))));
                        break;
                    }

                    case StylePropertyId.PreferredHeight:
                    case StylePropertyId.MinHeight:
                    case StylePropertyId.MaxHeight: {
                        float v0 = ResolveHeightMeasurement(target, viewport, prev.value.styleProperty.AsUIMeasurement);

                        float v1 = ResolveHeightMeasurement(target, viewport,next.value.styleProperty.AsUIMeasurement);

                        target.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(Mathf.Lerp(v0, v1, t))));
                        break;
                    }

                    case StylePropertyId.Opacity:
                    case StylePropertyId.ShadowOpacity:
                    case StylePropertyId.TextUnderlayDilate:
                    case StylePropertyId.TextUnderlaySoftness:
                    case StylePropertyId.TextUnderlayX:
                    case StylePropertyId.TextUnderlayY:
                    case StylePropertyId.TextFaceDilate:
                    case StylePropertyId.TextGlowInner:
                    case StylePropertyId.TextGlowOuter:
                    case StylePropertyId.TextGlowOffset:
                    case StylePropertyId.TextGlowPower:
                    case StylePropertyId.TextOutlineSoftness:
                    case StylePropertyId.TextOutlineWidth:

                    case StylePropertyId.ShadowIntensity:
                    case StylePropertyId.ShadowSizeX:
                    case StylePropertyId.ShadowSizeY:
                    case StylePropertyId.TransformScaleX:
                    case StylePropertyId.TransformScaleY:
                    case StylePropertyId.TransformRotation:
                    case StylePropertyId.GridLayoutColGap:
                    case StylePropertyId.GridLayoutRowGap: {
                        float v0 = prev.value.styleProperty.AsFloat;

                        float v1 =  next.value.styleProperty.AsFloat;

                        target.style.SetAnimatedProperty(new StyleProperty(propertyId, Mathf.Lerp(v0, v1, t)));
                        break;
                    }

                    case StylePropertyId.ShadowOffsetX:
                    case StylePropertyId.TransformPositionX:
                    case StylePropertyId.AlignmentOffsetX: {
                        Rect viewRect = target.View.Viewport;
                        if (target.layoutBox != null) {
                            float v0 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, prev.value.styleProperty.AsOffsetMeasurement, target.layoutResult.actualSize.width);
                            float v1 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, next.value.styleProperty.AsOffsetMeasurement, target.layoutResult.actualSize.width);
                            
                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, new OffsetMeasurement(Mathf.Lerp(v0, v1, t))));
                        }

                        break;
                    }

                    case StylePropertyId.ShadowOffsetY:
                    case StylePropertyId.TransformPositionY:
                    case StylePropertyId.AlignmentOffsetY: {
                        Rect viewRect = target.View.Viewport;
                        if (target.layoutBox != null) {
                            float v0 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, prev.value.styleProperty.AsOffsetMeasurement, target.layoutResult.actualSize.height);
                            float v1 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, next.value.styleProperty.AsOffsetMeasurement, target.layoutResult.actualSize.height);
                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, new OffsetMeasurement(Mathf.Lerp(v0, v1, t))));
                        }

                        break;
                    }

                    case StylePropertyId.AlignmentOriginX: {
                        Rect viewRect = target.View.Viewport;

                        if (target.layoutBox != null) {
                            float originSize = MeasurementUtil.ResolveOffsetOriginSizeX(target.layoutResult, viewRect.width, target.style.AlignmentTargetX);
                            float v0 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, prev.value.styleProperty.AsOffsetMeasurement, originSize);
                            float v1 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, next.value.styleProperty.AsOffsetMeasurement, originSize);
                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, new OffsetMeasurement(Mathf.Lerp(v0, v1, t))));
                        }

                        break;
                    }

                    case StylePropertyId.AlignmentOriginY: {
                        Rect viewRect = target.View.Viewport;

                        if (target.layoutBox != null) {
                            float originSize = MeasurementUtil.ResolveOffsetOriginSizeY(target.layoutResult, viewRect.width, target.style.AlignmentTargetY);
                            float v0 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, prev.value.styleProperty.AsOffsetMeasurement, originSize);
                            float v1 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, next.value.styleProperty.AsOffsetMeasurement, originSize);
                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, new OffsetMeasurement(Mathf.Lerp(v0, v1, t))));
                        }

                        break;
                    }

                    case StylePropertyId.TextUnderlayColor:
                    case StylePropertyId.TextGlowColor:
                    case StylePropertyId.TextOutlineColor:
                    case StylePropertyId.BorderColorTop:
                    case StylePropertyId.BorderColorRight:
                    case StylePropertyId.BorderColorBottom:
                    case StylePropertyId.BorderColorLeft:
                    case StylePropertyId.BackgroundColor:
                    case StylePropertyId.BackgroundTint:
                    case StylePropertyId.ShadowColor:
                    case StylePropertyId.ShadowTint:
                    case StylePropertyId.TextColor: {
                        Color c0 = prev.value.styleProperty.AsColor;
                        Color c1 = next.value.styleProperty.AsColor;
                        target.style.SetAnimatedProperty(new StyleProperty(propertyId, Color.Lerp(c0, c1, t)));
                        break;
                    }

                    default:
                        if (StyleUtil.CanAnimate(propertyId)) {
                            throw new NotImplementedException(propertyId + " can be animated but is not implemented");
                        }

                        throw new InvalidArgumentException(propertyId + " is not a supported animation property");
                }
            }

            if (progress >= 1f) {
                target.style.SetAnimatedProperty(next.value.styleProperty);
                return UITaskResult.Completed;
            }

            return UITaskResult.Running;
        }

    }

}
