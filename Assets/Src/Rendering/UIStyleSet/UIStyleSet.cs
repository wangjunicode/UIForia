using System;
using System.Collections.Generic;
using System.Diagnostics;
using Src;
using Src.Animation;
using Src.Elements;
using Src.Layout;
using Src.Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Rendering {

    [DebuggerDisplay("id = {element.id} state = {currentState}")]
    public partial class UIStyleSet {

        private string content;
        private int baseCounter;
        private StyleState currentState;
        private StyleEntry[] appliedStyles;
        private StyleState containedStates;

        public readonly UIElement element;
        public readonly ComputedStyle computedStyle;

        internal IStyleSystem styleSystem;

        public HorizontalScrollbarAttachment horizontalScrollbarAttachment => HorizontalScrollbarAttachment.Bottom;
        public VerticalScrollbarAttachment verticalScrollbarAttachment => VerticalScrollbarAttachment.Right;

        private static readonly HashSet<StylePropertyId> s_DefinedMap = new HashSet<StylePropertyId>();

        public UIStyleSet(UIElement element, IStyleSystem styleSystem) {
            this.element = element;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.styleSystem = styleSystem;
            this.computedStyle = new ComputedStyle(this);
        }

        public string textContent {
            get { return content; }
            set {
                content = value;
//                switch (whiteSpace) {
//                    case WhitespaceMode.Unset:
//                        content = value;
//                        break;
//                    case WhitespaceMode.Wrap:
//                        content = WhitespaceProcessor.ProcessWrap(value);
//                        break;
//                    case WhitespaceMode.NoWrap:
//                        content = value;
//                        break;
//                    case WhitespaceMode.Preserve:
//                        content = value;
//                        break;
//                    case WhitespaceMode.PreserveWrap:
//                        content = value;
//                        break;
//                    default:
//                        throw new ArgumentOutOfRangeException();
//                }
            }
        }

        public void EnterState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) != 0) {
                return;
            }

            currentState |= state;

            if (appliedStyles == null || (containedStates & state) == 0) {
                return;
            }

            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state != state)) {
                    continue;
                }

                UpdateStyleProperties(appliedStyles[i].style);
            }
        }

        public bool IsInState(StyleState state) {
            return (currentState & state) != 0;
        }

        public void ExitState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) == 0) {
                return;
            }

            currentState &= ~(state);
            currentState |= StyleState.Normal;

            if (appliedStyles == null || (containedStates & state) == 0) {
                return;
            }

            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state != state)) {
                    continue;
                }

                UpdateStyleProperties(appliedStyles[i].style);
            }
        }

        public bool HasHoverStyle => (containedStates & StyleState.Hover) != 0;

//        public UIStyleProxy hover {
//            get { return new UIStyleProxy(this, StyleState.Hover); }
//            // ReSharper disable once ValueParameterNotUsed
//            set { SetHoverStyle(UIStyleProxy.hack); }
//        }
//
//        public UIStyleProxy active {
//            get { return new UIStyleProxy(this, StyleState.Active); }
//            // ReSharper disable once ValueParameterNotUsed
//            set { SetActiveStyle(UIStyleProxy.hack); }
//        }
//
//        public UIStyleProxy focused {
//            get { return new UIStyleProxy(this, StyleState.Focused); }
//            // ReSharper disable once ValueParameterNotUsed
//            set { SetFocusedStyle(UIStyleProxy.hack); }
//        }
//
//        public UIStyleProxy disabled {
//            get { return new UIStyleProxy(this, StyleState.Disabled); }
//            // ReSharper disable once ValueParameterNotUsed
//            set { SetDisabledStyle(UIStyleProxy.hack); }
//        }

        public void PlayAnimation(string name) { }

        public void PlayAnimation(StyleAnimation animation) {
            styleSystem.PlayAnimation(this, animation, default(AnimationOptions));
        }

        public bool HandlesOverflow => computedStyle.OverflowX != Overflow.None || computedStyle.OverflowY != Overflow.None;

        public bool HandlesOverflowX => computedStyle.OverflowX != Overflow.None;

        public bool HandlesOverflowY => computedStyle.OverflowY != Overflow.None;

        public bool HasBaseStyles {
            get {
                if (appliedStyles == null) return false;
                for (int i = 0; i < appliedStyles.Length; i++) {
                    if (appliedStyles[i].type == StyleType.Shared) {
                        return true;
                    }
                }

                return false;
            }
        }

        public string GetBaseStyleNames() {
            string output = string.Empty;
            if (appliedStyles == null) return output;
            for (int i = 0; i < appliedStyles.Length; i++) {
                if (appliedStyles[i].type == StyleType.Shared) {
                    output += appliedStyles[i].style.Id;
                }
            }

            return output;
        }

        public void OnAnimationStart() { }

        public void SetNormalStyle(UIStyle style) {
            SetInstanceStyle(style, StyleState.Normal);
        }

        public void SetActiveStyle(UIStyle style) {
            SetInstanceStyle(style, StyleState.Active);
        }

        public void SetFocusedStyle(UIStyle style) {
            SetInstanceStyle(style, StyleState.Focused);
        }

        public void SetHoverStyle(UIStyle style) {
            SetInstanceStyle(style, StyleState.Hover);
        }

        public void SetDisabledStyle(UIStyle style) {
            SetInstanceStyle(style, StyleState.Disabled);
        }

        private void SetInstanceStyle(UIStyle style, StyleState state) {
            containedStates |= state;

            for (int i = 0; i < appliedStyles.Length; i++) {
                StyleState target = appliedStyles[i].state & state;
                if ((target != state) || appliedStyles[i].type != StyleType.Instance) {
                    continue;
                }

                UIStyle oldStyle = appliedStyles[i].style;
                appliedStyles[i] = new StyleEntry(new UIStyle(style), StyleType.Instance, state);

                if (IsInState(state)) {
                    UpdateStyleProperties(oldStyle);
                    UpdateStyleProperties(style);
                }

                return;
            }

            Array.Resize(ref appliedStyles, appliedStyles.Length + 1);
            appliedStyles[appliedStyles.Length - 1] = new StyleEntry(style, StyleType.Instance, state);
            SortStyles();
            if (IsInState(state)) {
                UpdateStyleProperties(style);
            }
        }

        private void UpdateStyleProperties(UIStyle style) {
            IReadOnlyList<StyleProperty> properties = style.Properties;
            for (int j = 0; j < properties.Count; j++) {
                computedStyle.SetProperty(GetPropertyValueInState(properties[j].propertyId, currentState));
            }
        }

        public void AddBaseStyleGroup(UIBaseStyleGroup group) {
            if (group.normal != null) AddBaseStyle(group.normal, StyleState.Normal);
            if (group.active != null) AddBaseStyle(group.active, StyleState.Active);
            if (group.disabled != null) AddBaseStyle(group.disabled, StyleState.Disabled);
            if (group.focused != null) AddBaseStyle(group.focused, StyleState.Focused);
            if (group.hover != null) AddBaseStyle(group.hover, StyleState.Hover);
        }

        public void AddBaseStyle(UIStyle style, StyleState state) {
            // todo -- check for duplicates
            containedStates |= state;
            if (appliedStyles == null) {
                appliedStyles = new StyleEntry[1];
            }
            else {
                Array.Resize(ref appliedStyles, appliedStyles.Length + 1);
            }

            appliedStyles[appliedStyles.Length - 1] = new StyleEntry(style, StyleType.Shared, state, baseCounter++);
            SortStyles();
            if (IsInState(state)) {
                UpdateStyleProperties(style);
            }
        }

        public void RemoveBaseStyle(UIStyle style, StyleState state = StyleState.Normal) {
            if (appliedStyles == null) {
                return;
            }

            for (int i = 0; i < appliedStyles.Length; i++) {
                if (appliedStyles[i].style == style && state == appliedStyles[i].state) {
                    appliedStyles[i] = appliedStyles[appliedStyles.Length - 1];
                    break;
                }
            }

            Array.Resize(ref appliedStyles, appliedStyles.Length - 1);
            SortStyles();

            for (int i = 0; i < appliedStyles.Length; i++) {
                containedStates |= appliedStyles[i].state;
            }

            if (IsInState(state)) {
                UpdateStyleProperties(style);
            }
        }

        private void SortStyles() {
            Array.Sort(appliedStyles, (a, b) => a.priority > b.priority ? -1 : 1);
        }

        public StyleProperty GetPropertyValue(StylePropertyId propertyId) {
            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((int) appliedStyles[i].state != -1 && (appliedStyles[i].state & currentState) == 0) {
                    continue;
                }

                StyleProperty property = appliedStyles[i].style.FindProperty(propertyId);
                if (property.IsDefined) {
                    return property;
                }
            }

            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue);
        }

        private StyleProperty GetPropertyValueInState(StylePropertyId propertyId, StyleState state) {
            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & state) == 0) {
                    continue;
                }

                StyleProperty property = appliedStyles[i].style.FindProperty(propertyId);
                if (property.IsDefined) {
                    return property;
                }
            }

            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue);
        }

        private UIStyle GetInstanceStyle(StyleState state) {
            for (int i = 0; i < appliedStyles.Length; i++) {
                StyleState checkFlag = appliedStyles[i].state;
                UIStyle style = appliedStyles[i].style;

                if ((checkFlag & state) != 0 && appliedStyles[i].type == StyleType.Instance) {
                    return style;
                }
            }

            return null;
        }

        private UIStyle GetOrCreateInstanceStyle(StyleState state) {
            if (appliedStyles == null) {
                UIStyle newStyle = new UIStyle();
                appliedStyles = new[] {
                    new StyleEntry(newStyle, StyleType.Instance, state),
                };
                containedStates |= state;
                return newStyle;
            }

            UIStyle retn = GetInstanceStyle(state);
            if (retn != null) {
                return retn;
            }

            UIStyle style = new UIStyle();
            SetInstanceStyle(style, state);
            return style;
        }

        internal void Initialize() {
            if (appliedStyles == null) return;

            for (int i = 0; i < appliedStyles.Length; i++) {
                StyleEntry entry = appliedStyles[i];
                containedStates |= entry.state;
                if ((entry.state & currentState) == 0) {
                    continue;
                }

                IReadOnlyList<StyleProperty> properties = entry.style.Properties;
                for (int j = 0; j < properties.Count; j++) {
                    if (!s_DefinedMap.Contains(properties[j].propertyId)) {
                        s_DefinedMap.Add(properties[j].propertyId);
                        computedStyle.SetProperty(properties[j]);
                    }
                }
            }

            s_DefinedMap.Clear();
        }

        private float GetFloatValue(StylePropertyId propertyId, StyleState state) {
            StyleProperty property = GetPropertyValueInState(propertyId, state);
            return property.IsDefined
                ? FloatUtil.DecodeToFloat(property.valuePart0)
                : FloatUtil.UnsetValue;
        }

        private Color GetColorValue(StylePropertyId propertyId, StyleState state) {
            StyleProperty property = GetPropertyValueInState(propertyId, state);
            return property.IsDefined ? (Color) new StyleColor(property.valuePart0) : ColorUtil.UnsetValue;
        }

        private int GetIntValue(StylePropertyId propertyId, StyleState state) {
            StyleProperty property = GetPropertyValueInState(propertyId, state);
            return property.IsDefined
                ? property.valuePart0
                : IntUtil.UnsetValue;
        }

        private int GetEnumProperty(StylePropertyId propertyId, StyleState state) {
            StyleProperty property = GetPropertyValueInState(propertyId, state);
            return property.IsDefined
                ? property.valuePart0
                : IntUtil.UnsetValue;
        }

        private UIMeasurement GetUIMeasurementValue(StylePropertyId propertyId, StyleState state) {
            StyleProperty property = GetPropertyValueInState(propertyId, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        private UIStyle GetActiveStyleForProperty(StylePropertyId stylePropertyId) {
            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & currentState) == 0) {
                    continue;
                }

                if ((appliedStyles[i].style.DefinesProperty(stylePropertyId))) {
                    return appliedStyles[i].style;
                }
            }

            return null;
        }

        private void SetAssetPointerProperty(StylePropertyId propertyId, AssetPointer<Font> assetPointer, StyleState state) {
            throw new NotImplementedException();
        }

        private void SetColorProperty(StylePropertyId propertyId, Color color, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.SetColorProperty(propertyId, color);
            if ((state & currentState) == 0) {
                return;
            }

            computedStyle.SetProperty(GetPropertyValue(propertyId));
        }

        private void SetUIMeasurementProperty(StylePropertyId propertyId, UIMeasurement measurement, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.SetUIMeasurementProperty(propertyId, measurement);
            if ((state & currentState) == 0) {
                return;
            }

            computedStyle.SetProperty(GetPropertyValue(propertyId));
        }

        private void SetIntProperty(StylePropertyId propertyId, int value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.SetIntProperty(propertyId, value);
            if ((state & currentState) == 0) {
                return;
            }

            computedStyle.SetProperty(GetPropertyValue(propertyId));
        }

        private void SetEnumProperty(StylePropertyId propertyId, int value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.SetEnumProperty(propertyId, value);
            if ((state & currentState) == 0) {
                return;
            }

            computedStyle.SetProperty(GetPropertyValue(propertyId));
        }

        internal void InitializeScrollbar(VirtualScrollbar scrollbar) {
            UIStyleSet styleSet = new UIStyleSet(scrollbar, styleSystem);
            styleSet.SetBackgroundColor(Color.green, StyleState.Normal);
            styleSet.SetBackgroundColor(Color.yellow, StyleState.Hover);
            styleSet.SetLayoutBehavior(LayoutBehavior.Ignored, StyleState.Normal);
            scrollbar.style = styleSet;
            UIStyleSet handleStyle = new UIStyleSet(scrollbar.handle, styleSystem);
            handleStyle.SetBackgroundColor(Color.blue, StyleState.Normal);
            handleStyle.SetBackgroundColor(Color.black, StyleState.Hover);
            handleStyle.SetLayoutBehavior(LayoutBehavior.Ignored, StyleState.Normal);
            scrollbar.handle.style = handleStyle;
        }

        public void SetProperty(StyleProperty property, StyleState state) {
            throw new NotImplementedException();
            switch (property.propertyId) {
                case StylePropertyId.OverflowX:
                    break;
                case StylePropertyId.OverflowY:
                    break;
                case StylePropertyId.BackgroundColor:
                    break;
                case StylePropertyId.BorderColor:
                    break;
                case StylePropertyId.BackgroundImage:
                    break;
                case StylePropertyId.GridItemColStart:
                    break;
                case StylePropertyId.GridItemColSpan:
                    break;
                case StylePropertyId.GridItemRowStart:
                    break;
                case StylePropertyId.GridItemRowSpan:
                    break;
                case StylePropertyId.GridLayoutDirection:
                    break;
                case StylePropertyId.GridLayoutDensity:
                    break;
                case StylePropertyId.GridLayoutColTemplate:
                    break;
                case StylePropertyId.GridLayoutRowTemplate:
                    break;
                case StylePropertyId.GridLayoutColAutoSize:
                    break;
                case StylePropertyId.GridLayoutRowAutoSize:
                    break;
                case StylePropertyId.GridLayoutColGap:
                    break;
                case StylePropertyId.GridLayoutRowGap:
                    break;
                case StylePropertyId.GridLayoutColAlignment:
                    break;
                case StylePropertyId.GridLayoutRowAlignment:
                    break;
                case StylePropertyId.FlexLayoutWrap:
                    break;
                case StylePropertyId.FlexLayoutDirection:
                    break;
                case StylePropertyId.FlexLayoutMainAxisAlignment:
                    break;
                case StylePropertyId.FlexLayoutCrossAxisAlignment:
                    break;
                case StylePropertyId.FlexItemSelfAlignment:
                    break;
                case StylePropertyId.FlexItemOrder:
                    break;
                case StylePropertyId.FlexItemGrow:
                    break;
                case StylePropertyId.FlexItemShrink:
                    break;
                case StylePropertyId.MarginTop:
                    break;
                case StylePropertyId.MarginRight:
                    break;
                case StylePropertyId.MarginBottom:
                    break;
                case StylePropertyId.MarginLeft:
                    break;
                case StylePropertyId.BorderTop:
                    break;
                case StylePropertyId.BorderRight:
                    break;
                case StylePropertyId.BorderBottom:
                    break;
                case StylePropertyId.BorderLeft:
                    break;
                case StylePropertyId.PaddingTop:
                    break;
                case StylePropertyId.PaddingRight:
                    break;
                case StylePropertyId.PaddingBottom:
                    break;
                case StylePropertyId.PaddingLeft:
                    break;
                case StylePropertyId.BorderRadiusTopLeft:
                    break;
                case StylePropertyId.BorderRadiusTopRight:
                    break;
                case StylePropertyId.BorderRadiusBottomLeft:
                    break;
                case StylePropertyId.BorderRadiusBottomRight:
                    break;
                case StylePropertyId.TransformPositionX:
                    break;
                case StylePropertyId.TransformPositionY:
                    break;
                case StylePropertyId.TransformScaleX:
                    break;
                case StylePropertyId.TransformScaleY:
                    break;
                case StylePropertyId.TransformPivotX:
                    break;
                case StylePropertyId.TransformPivotY:
                    break;
                case StylePropertyId.TransformRotation:
                    break;
                case StylePropertyId.__TextPropertyStart__:
                    break;
                case StylePropertyId.TextColor:
                    break;
                case StylePropertyId.TextFontAsset:
                    break;
                case StylePropertyId.TextFontSize:
                    break;
                case StylePropertyId.TextFontStyle:
                    break;
                case StylePropertyId.TextAnchor:
                    break;
                case StylePropertyId.TextWhitespaceMode:
                    break;
                case StylePropertyId.TextWrapMode:
                    break;
                case StylePropertyId.TextHorizontalOverflow:
                    break;
                case StylePropertyId.TextVerticalOverflow:
                    break;
                case StylePropertyId.TextIndentFirstLine:
                    break;
                case StylePropertyId.TextIndentNewLine:
                    break;
                case StylePropertyId.TextLayoutStyle:
                    break;
                case StylePropertyId.TextAutoSize:
                    break;
                case StylePropertyId.TextTransform:
                    break;
                case StylePropertyId.__TextPropertyEnd__:
                    break;
                case StylePropertyId.MinWidth:
                    break;
                case StylePropertyId.MaxWidth:
                    break;
                case StylePropertyId.PreferredWidth:
                    break;
                case StylePropertyId.MinHeight:
                    break;
                case StylePropertyId.MaxHeight:
                    break;
                case StylePropertyId.PreferredHeight:
                    break;
                case StylePropertyId.LayoutType:
                    break;
                case StylePropertyId.IsInLayoutFlow:
                    break;
                case StylePropertyId.LayoutBehavior:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // todo -- add an opacity property

        public void SetAnimatedProperty(StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.BackgroundColor:
                case StylePropertyId.BorderColor:
                case StylePropertyId.BackgroundImage:
                case StylePropertyId.MarginTop:
                case StylePropertyId.MarginRight:
                case StylePropertyId.MarginBottom:
                case StylePropertyId.MarginLeft:
                case StylePropertyId.BorderTop:
                case StylePropertyId.BorderRight:
                case StylePropertyId.BorderBottom:
                case StylePropertyId.BorderLeft:
                case StylePropertyId.PaddingTop:
                case StylePropertyId.PaddingRight:
                case StylePropertyId.PaddingBottom:
                case StylePropertyId.PaddingLeft:
                case StylePropertyId.BorderRadiusTopLeft:
                case StylePropertyId.BorderRadiusTopRight:
                case StylePropertyId.BorderRadiusBottomLeft:
                case StylePropertyId.BorderRadiusBottomRight:
                case StylePropertyId.TransformPositionX:
                case StylePropertyId.TransformPositionY:
                case StylePropertyId.TransformScaleX:
                case StylePropertyId.TransformScaleY:
                case StylePropertyId.TransformPivotX:
                case StylePropertyId.TransformPivotY:
                case StylePropertyId.TransformRotation:
                case StylePropertyId.TextColor:
                case StylePropertyId.TextFontSize:
                case StylePropertyId.TextIndentFirstLine:
                case StylePropertyId.TextIndentNewLine:
                case StylePropertyId.PreferredWidth:
                case StylePropertyId.PreferredHeight:
                case StylePropertyId.AnchorTop:
                case StylePropertyId.AnchorRight:
                case StylePropertyId.AnchorBottom:
                case StylePropertyId.AnchorLeft:
                    computedStyle.SetProperty(property);
                    break;
                default:
                    throw new Exception(property.propertyId + " is not able to be animated");
            }
        }

    }

}