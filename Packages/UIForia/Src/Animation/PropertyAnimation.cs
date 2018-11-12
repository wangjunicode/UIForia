using System;
using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Animation {

    public struct AnimationState {

        public int iterationCount;
        public float elapsedTime;
        public float floatValue;
        public object startValueAsObject;
        public AnimationDirection direction;
        public bool isDelaying;

        public AnimationState(float floatValue, object value) {
            this.elapsedTime = 0;
            this.iterationCount = 0;
            this.startValueAsObject = value;
            this.floatValue = floatValue;
            this.direction = AnimationDirection.Forward;
            this.isDelaying = false;
        }

        public AnimationState(object value) {
            this.elapsedTime = 0;
            this.iterationCount = 0;
            this.floatValue = 0;
            this.startValueAsObject = value;
            this.direction = AnimationDirection.Forward;
            this.isDelaying = false;
        }

        public AnimationState(float value) {
            this.iterationCount = 0;
            this.elapsedTime = 0;
            this.floatValue = value;
            this.startValueAsObject = null;
            this.direction = AnimationDirection.Forward;
            this.isDelaying = false;
        }

    }

    public class PropertyAnimation : StyleAnimation {

        public StyleProperty m_StartValue;
        public StyleProperty m_TargetValue;
        private readonly List<ValueTuple<int, AnimationState>> m_StatusList;

        public PropertyAnimation(StyleProperty targetValue, AnimationOptions options) {
            m_Options = options;
            m_TargetValue = targetValue;
            m_StartValue = StyleProperty.Unset(m_TargetValue.propertyId);
            m_StatusList = ListPool<ValueTuple<int, AnimationState>>.Get();
        }

        public PropertyAnimation(StyleProperty startValue, StyleProperty targetValue, AnimationOptions options) {
            m_Options = options;
            m_StartValue = startValue;
            m_TargetValue = targetValue;
            m_StatusList = ListPool<ValueTuple<int, AnimationState>>.Get();
        }

        public override void OnStart(UIStyleSet styleSet, Rect viewport) {
            UIElement element = styleSet.element;
            int idx = -1;
            for (int i = 0; i < m_StatusList.Count; i++) {
                if (m_StatusList[i].Item1 == element.id) {
                    idx = i;
                    break;
                }
            }

            if (idx == -1) {
                StyleProperty startValue;
                if (m_StartValue.IsDefined) {
                    startValue = m_StartValue;
                }
                else {
                    startValue = styleSet.computedStyle.GetProperty(m_TargetValue.propertyId);
                }

                AnimationState status = ResolveStartValue(styleSet, viewport, startValue);
                if (m_Options.direction == AnimationDirection.Reverse) {
                    status.direction = AnimationDirection.Reverse;
                }

                if (m_Options.delay > 0) {
                    status.isDelaying = true;
                }

                m_StatusList.Add(ValueTuple.Create(element.id, status));
            }
            else {
                throw new Exception("Trying to start the same animation twice: " + element);
            }
        }

        private AnimationState ResolveStartValue(UIStyleSet styleSet, Rect viewport, StyleProperty startProperty) {
            UIElement element = styleSet.element;
            switch (m_TargetValue.propertyId) {
                case StylePropertyId.TransformPivotX:
                case StylePropertyId.TransformPositionX:
                case StylePropertyId.PaddingLeft:
                case StylePropertyId.PaddingRight:
                case StylePropertyId.BorderLeft:
                case StylePropertyId.BorderRight:
                case StylePropertyId.MarginLeft:
                case StylePropertyId.MarginRight:
                    return new AnimationState(ResolveFixedWidth(element, viewport, startProperty.AsUIFixedLength));

                case StylePropertyId.TransformPivotY:
                case StylePropertyId.TransformPositionY:
                case StylePropertyId.PaddingTop:
                case StylePropertyId.PaddingBottom:
                case StylePropertyId.BorderTop:
                case StylePropertyId.BorderBottom:
                case StylePropertyId.MarginTop:
                case StylePropertyId.MarginBottom:
                    return new AnimationState(ResolveFixedHeight(element, viewport, startProperty.AsUIFixedLength));

                case StylePropertyId.PreferredWidth:
                    return new AnimationState(ResolveWidthMeasurement(element, viewport, startProperty.AsUIMeasurement));

                case StylePropertyId.PreferredHeight:
                    return new AnimationState(ResolveHeightMeasurement(element, viewport, startProperty.AsUIMeasurement));

                case StylePropertyId.Opacity:
                case StylePropertyId.TransformScaleX:
                case StylePropertyId.TransformScaleY:
                case StylePropertyId.TransformRotation:
                case StylePropertyId.GridLayoutColGap:
                case StylePropertyId.GridLayoutRowGap:
                    return new AnimationState(startProperty.AsFloat);

                case StylePropertyId.AnchorTop:
                    return new AnimationState(ResolveAnchorTop(element, viewport, startProperty.AsUIFixedLength));

                case StylePropertyId.AnchorRight:
                    return new AnimationState(ResolveAnchorRight(element, viewport, startProperty.AsUIFixedLength));

                case StylePropertyId.AnchorBottom:
                    return new AnimationState(ResolveAnchorBottom(element, viewport, startProperty.AsUIFixedLength));

                case StylePropertyId.AnchorLeft:
                    return new AnimationState(ResolveAnchorLeft(element, viewport, startProperty.AsUIFixedLength));

                case StylePropertyId.BorderColor:
                case StylePropertyId.BackgroundColor:
                case StylePropertyId.TextColor:
                    // todo gradient works differently now
//                    if (startProperty.IsGradient) {
//                        return new AnimationStatus((int) ColorType.Gradient, startProperty.AsGradient);
//                    }
//                    else {
                        return new AnimationState((int) ColorType.Color, new StyleColor(startProperty.AsColor).rgba);
//                    }

                default: throw new UIForia.InvalidArgumentException(m_TargetValue.propertyId + " is not a supported animation property");
            }
        }

        public override AnimationStatus Update(UIStyleSet styleSet, Rect viewport, float deltaTime) {
            UIElement element = styleSet.element;
            int idx = -1;
            for (int i = 0; i < m_StatusList.Count; i++) {
                if (m_StatusList[i].Item1 == element.id) {
                    idx = i;
                    break;
                }
            }

            if (idx == -1) {
                // invalid
                return AnimationStatus.Completed;
            }

            float duration = (m_Options.duration - m_Options.delay) / Mathf.Max(1, m_Options.iterations);

            AnimationState status = m_StatusList[idx].Item2;
            status.elapsedTime += deltaTime;
            StylePropertyId propertyId = m_TargetValue.propertyId;

            if (status.iterationCount == 0) {
                if (status.isDelaying) {
                    if (status.elapsedTime < m_Options.delay) {
                        m_StatusList[idx] = ValueTuple.Create(element.id, status);
                        return AnimationStatus.Running;
                    }
                    else {
                        status.isDelaying = false;
                        status.elapsedTime = 0f;
                    }
                }
            }
            else {
                if (status.isDelaying) {
                    if (status.direction == AnimationDirection.Forward) {
                        if (status.elapsedTime >= m_Options.forwardStartDelay) {
                            status.elapsedTime = 0f;
                            status.isDelaying = false;
                        }
                        else {
                            m_StatusList[idx] = ValueTuple.Create(element.id, status);
                            return AnimationStatus.Running;
                        }
                    }
                    else {
                        if (status.elapsedTime >= m_Options.reverseStartDelay) {
                            status.elapsedTime = 0f;
                            status.isDelaying = false;
                        }
                        else {
                            m_StatusList[idx] = ValueTuple.Create(element.id, status);
                            return AnimationStatus.Running;
                        }
                    }
                }
            }

            float t = Mathf.Clamp01(Easing.Interpolate(status.elapsedTime / duration, m_Options.timingFunction));
            float v;

            float adjustedT = t;

            if (status.direction == AnimationDirection.Reverse) {
                adjustedT = 1 - t;
            }

            switch (propertyId) {
                case StylePropertyId.TransformPivotX:
                case StylePropertyId.TransformPositionX:
                case StylePropertyId.PaddingLeft:
                case StylePropertyId.PaddingRight:
                case StylePropertyId.BorderLeft:
                case StylePropertyId.BorderRight:
                case StylePropertyId.MarginLeft:
                case StylePropertyId.MarginRight:
                    v = Mathf.Lerp(status.floatValue, ResolveFixedWidth(element, viewport, m_TargetValue.AsUIFixedLength), adjustedT);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(v)));
                    break;
                case StylePropertyId.TransformPivotY:
                case StylePropertyId.TransformPositionY:
                case StylePropertyId.PaddingTop:
                case StylePropertyId.PaddingBottom:
                case StylePropertyId.BorderTop:
                case StylePropertyId.BorderBottom:
                case StylePropertyId.MarginTop:
                case StylePropertyId.MarginBottom:
                    v = Mathf.Lerp(status.floatValue, ResolveFixedHeight(element, viewport, m_TargetValue.AsUIFixedLength), adjustedT);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(v)));
                    break;
                case StylePropertyId.PreferredWidth:
                    v = Mathf.Lerp(status.floatValue, ResolveWidthMeasurement(element, viewport, m_TargetValue.AsUIMeasurement), adjustedT);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(v)));
                    break;
                case StylePropertyId.PreferredHeight:
                    v = Mathf.Lerp(status.floatValue, ResolveHeightMeasurement(element, viewport, m_TargetValue.AsUIMeasurement), adjustedT);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(v)));
                    break;
                case StylePropertyId.Opacity:
                case StylePropertyId.TransformScaleX:
                case StylePropertyId.TransformScaleY:
                case StylePropertyId.TransformRotation:
                case StylePropertyId.GridLayoutColGap:
                case StylePropertyId.GridLayoutRowGap:
                    v = Mathf.Lerp(status.floatValue, m_TargetValue.AsFloat, adjustedT);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, v));
                    break;
                case StylePropertyId.AnchorTop:
                    v = Mathf.Lerp(status.floatValue, ResolveAnchorTop(element, viewport, m_TargetValue.AsUIFixedLength), adjustedT);
                    element.style.SetAnimatedProperty(StyleProperty.AnchorTop(v));
                    break;

                case StylePropertyId.AnchorRight:
                    float val = ResolveAnchorRight(element, viewport, m_TargetValue.AsUIFixedLength);
                    v = Mathf.Lerp(status.floatValue, val, adjustedT);
                    element.style.SetAnimatedProperty(StyleProperty.AnchorRight(v));
                    break;

                case StylePropertyId.AnchorBottom:
                    v = Mathf.Lerp(status.floatValue, ResolveAnchorBottom(element, viewport, m_TargetValue.AsUIFixedLength), adjustedT);
                    element.style.SetAnimatedProperty(StyleProperty.AnchorBottom(v));
                    break;

                case StylePropertyId.AnchorLeft:
                    v = Mathf.Lerp(status.floatValue, ResolveAnchorLeft(element, viewport, m_TargetValue.AsUIFixedLength), adjustedT);
                    element.style.SetAnimatedProperty(StyleProperty.AnchorLeft(v));
                    break;

                case StylePropertyId.BorderColor:
                case StylePropertyId.BackgroundColor:
                case StylePropertyId.TextColor:
//                    if (m_TargetValue.IsGradient) {
//                        Rendering.Gradient targetGradient = m_TargetValue.AsGradient;
//                        if ((int) status.floatValue == (int) ColorType.Gradient) {
//                            Rendering.Gradient startGradient = (Rendering.Gradient) status.startValueAsObject;
//                            Rendering.Gradient finalGradient = new Rendering.Gradient();
//
//                            finalGradient.start = Mathf.Lerp(startGradient.start, targetGradient.start, adjustedT);
//                            finalGradient.rotation = Mathf.Lerp(startGradient.rotation, targetGradient.rotation, adjustedT);
//                            finalGradient.color0 = Color.Lerp(startGradient.color0, targetGradient.color0, adjustedT);
//                            finalGradient.color1 = Color.Lerp(startGradient.color1, targetGradient.color1, adjustedT);
//                            finalGradient.offset = Vector2.Lerp(startGradient.offset, targetGradient.offset, adjustedT);
//                            finalGradient.axis = targetGradient.axis;
//                            finalGradient.type = targetGradient.type;
//                            element.style.SetAnimatedProperty(new StyleProperty(propertyId, finalGradient));
//                        }
//                        else {
//                            Color color = new StyleColor((int) status.floatValue);
//
//                            Rendering.Gradient finalGradient = new Rendering.Gradient();
//
//                            finalGradient.start = Mathf.Lerp(0, targetGradient.start, adjustedT);
//                            finalGradient.rotation = Mathf.Lerp(0, targetGradient.rotation, adjustedT);
//                            finalGradient.color0 = Color.Lerp(color, targetGradient.color0, adjustedT);
//                            finalGradient.color1 = Color.Lerp(color, targetGradient.color1, adjustedT);
//                            finalGradient.offset = Vector2.Lerp(Vector2.zero, targetGradient.offset, adjustedT);
//                            finalGradient.axis = targetGradient.axis;
//                            finalGradient.type = targetGradient.type;
//
//                            element.style.SetAnimatedProperty(new StyleProperty(propertyId, finalGradient));
//                        }
//                    }
//                    else {
//                        if ((int) status.floatValue == (int) ColorType.Gradient) {
//                            Rendering.Gradient startGradient = (Rendering.Gradient) status.startValueAsObject;
//                            Rendering.Gradient finalGradient = new Rendering.Gradient();
//                            Color targetColor = m_TargetValue.AsColor;
//                            finalGradient.start = Mathf.Lerp(startGradient.start, 0, adjustedT);
//                            finalGradient.rotation = Mathf.Lerp(startGradient.rotation, 0, adjustedT);
//                            finalGradient.color0 = Color.Lerp(startGradient.color0, targetColor, adjustedT);
//                            finalGradient.color1 = Color.Lerp(startGradient.color1, targetColor, adjustedT);
//                            finalGradient.offset = Vector2.Lerp(startGradient.offset, Vector2.zero, adjustedT);
//                            finalGradient.axis = startGradient.axis;
//                            finalGradient.type = startGradient.type;
//                            element.style.SetAnimatedProperty(new StyleProperty(propertyId, finalGradient));
//                        }
//                        else {
//                            Color c = Color.Lerp(new StyleColor((int) status.floatValue), m_TargetValue.AsColor, adjustedT);
//                            element.style.SetAnimatedProperty(new StyleProperty(propertyId, c));
//                        }
//                    }

                    break;

                default:
                    throw new UIForia.InvalidArgumentException(propertyId + " is not a supported animation property");
            }

            if (t == 1f) {
                status.elapsedTime = 0f;
                status.iterationCount++;
                if (m_Options.loopType == AnimationLoopType.PingPong) {
                    if (status.direction == AnimationDirection.Forward) {
                        status.direction = AnimationDirection.Reverse;
                    }
                    else {
                        status.direction = AnimationDirection.Forward;
                    }
                }

                if (status.direction == AnimationDirection.Forward) {
                    if (m_Options.forwardStartDelay > 0f) {
                        status.isDelaying = true;
                    }
                }
                else {
                    if (m_Options.reverseStartDelay > 0f) {
                        status.isDelaying = true;
                    }
                }
            }

            m_StatusList[idx] = ValueTuple.Create(element.id, status);
            return status.iterationCount == Mathf.Max(1, m_Options.iterations) && t == 1 ? AnimationStatus.Completed : AnimationStatus.Running;
        }

        public override void OnEnd(UIStyleSet styleSet) {
            for (int i = 0; i < m_StatusList.Count; i++) {
                if (m_StatusList[i].Item1 == styleSet.element.id) {
                    m_StatusList.RemoveAt(i);
                    return;
                }
            }
        }

    }

}