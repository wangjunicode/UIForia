using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Animation {

    public class KeyFrameAnimation : StyleAnimation {

        private float elapsedTime;
        public AnimationOptions options;

        private readonly List<ValueTuple<StylePropertyId, List<ProcessedKeyFrame>>> processedFrames;

        public KeyFrameAnimation(AnimationOptions options, params AnimationKeyFrame[] frames) {
            this.options = options;
            processedFrames = ListPool<ValueTuple<StylePropertyId, List<ProcessedKeyFrame>>>.Get();

            for (int i = 0; i < frames.Length; i++) {
                AnimationKeyFrame f = frames[i];
                for (int j = 0; j < f.properties.Length; j++) {
                    AddKeyFrame(f.key, f.properties[j]);
                }
            }
        }

        private void AddKeyFrame(float time, StyleProperty property) {
            for (int i = 0; i < processedFrames.Count; i++) {
                if (processedFrames[i].Item1 == property.propertyId) {
                    processedFrames[i].Item2.Add(new ProcessedKeyFrame(time, property));
                    return;
                }
            }

            List<ProcessedKeyFrame> list = ListPool<ProcessedKeyFrame>.Get();
            list.Add(new ProcessedKeyFrame(time, property));
            processedFrames.Add(ValueTuple.Create(property.propertyId, list));
        }

        public override void OnStart(UIStyleSet styleSet, Rect viewport) {
            for (int i = 0; i < processedFrames.Count; i++) {
                StylePropertyId property = processedFrames[i].Item1;
                List<ProcessedKeyFrame> frames = processedFrames[i].Item2;
                if (frames[0].key != 0f) {
                    frames.Insert(0, new ProcessedKeyFrame(0f, styleSet.GetPropertyValue(property)));
                }
            }
        }
        
        public override AnimationStatus Update(UIStyleSet styleSet, Rect viewport, float deltaTime) {
            elapsedTime += deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / options.duration);
          
            UIElement element = styleSet.element;
            // todo - insert an implicit frame for 0% and 100% if not provided

            // todo handle delay, etc from options
            
            for (int i = 0; i < processedFrames.Count; i++) {
                StylePropertyId propertyId = processedFrames[i].Item1;
                List<ProcessedKeyFrame> frames = processedFrames[i].Item2;

                ProcessedKeyFrame prev = frames[0];
                ProcessedKeyFrame next = frames[frames.Count - 1];
                for (int j = 1; j < frames.Count; j++) {
                    if (frames[j].key > progress) {
                        prev = frames[j - 1];
                        next = frames[j];
                        break;
                    }
                }

                if (prev.key > (elapsedTime / options.duration)) {
                    continue;
                }

                float v0;
                float v1;
                float t = (((elapsedTime / options.duration) - prev.key) / (next.key - prev.key));
                
                switch (propertyId) {
                    case StylePropertyId.TransformPivotX:
                    case StylePropertyId.TransformPositionX:
                    case StylePropertyId.PaddingLeft:
                    case StylePropertyId.PaddingRight:
                    case StylePropertyId.BorderLeft:
                    case StylePropertyId.BorderRight:
                    case StylePropertyId.MarginLeft:
                    case StylePropertyId.MarginRight:
                        v0 = ResolveFixedWidth(element, viewport, prev.property.AsUIFixedLength);
                        v1 = ResolveFixedWidth(element, viewport, next.property.AsUIFixedLength);
                        element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(Mathf.Lerp(v0, v1, t))));
                        break;
                    case StylePropertyId.TransformPivotY:
                    case StylePropertyId.TransformPositionY:
                    case StylePropertyId.PaddingTop:
                    case StylePropertyId.PaddingBottom:
                    case StylePropertyId.BorderTop:
                    case StylePropertyId.BorderBottom:
                    case StylePropertyId.MarginTop:
                    case StylePropertyId.MarginBottom:
                        v0 = ResolveFixedHeight(element, viewport, prev.property.AsUIFixedLength);
                        v1 = ResolveFixedHeight(element, viewport, next.property.AsUIFixedLength);
                        element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(Mathf.Lerp(v0, v1, t))));
                        break;
                    case StylePropertyId.PreferredWidth:
                        v0 = ResolveWidthMeasurement(element, viewport, prev.property.AsUIMeasurement);
                        v1 = ResolveWidthMeasurement(element, viewport, next.property.AsUIMeasurement);
                        element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(Mathf.Lerp(v0, v1, t))));
                        break;
                    case StylePropertyId.PreferredHeight:
                        v0 = ResolveHeightMeasurement(element, viewport, prev.property.AsUIMeasurement);
                        v1 = ResolveHeightMeasurement(element, viewport, next.property.AsUIMeasurement);
                        element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(Mathf.Lerp(v0, v1, t))));
                        break;
                    case StylePropertyId.Opacity:
                    case StylePropertyId.TransformScaleX:
                    case StylePropertyId.TransformScaleY:
                    case StylePropertyId.TransformRotation:
                    case StylePropertyId.GridLayoutColGap:
                    case StylePropertyId.GridLayoutRowGap:
                        v0 = prev.property.AsFloat;
                        v1 = next.property.AsFloat;
                        element.style.SetAnimatedProperty(new StyleProperty(propertyId, Mathf.Lerp(v0, v1, t)));
                        break;
                    case StylePropertyId.BorderColor:
                    case StylePropertyId.BackgroundColor:
                    case StylePropertyId.TextColor:
                        // todo -- figure out gradients & lerping those
                        Color c0 = prev.property.AsColor;
                        Color c1 = next.property.AsColor;
                        element.style.SetAnimatedProperty(new StyleProperty(propertyId, Color.Lerp(c0, c1, t)));
                        break;

                    default:
                        throw new InvalidArgumentException(propertyId + " is not a supported animation property");
                }
            }
            
            return progress == 1f ? AnimationStatus.Completed : AnimationStatus.Running;
            
        }

        private struct ProcessedKeyFrame {

            public readonly float key;
            public readonly StyleProperty property;

            public ProcessedKeyFrame(float key, StyleProperty property) {
                this.key = key;
                this.property = property;
            }

        }

    }

}