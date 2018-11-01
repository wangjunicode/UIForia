using System;
using System.Collections.Generic;
using System.Diagnostics;
using Src;
using Src.Animation;
using Src.Elements;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Rendering {

    [DebuggerDisplay("id = {element.id} state = {currentState}")]
    public partial class UIStyleSet {

        private int baseCounter;
        private StyleState currentState;
        private UIStyleGroup instanceStyle;
        private LightList<StyleEntry> appliedStyles;
        private StyleState containedStates;
        private string styleNames;

        // temp -- replace w/ IntMap & remove computed style
        private List<UIStyleGroup> styleGroups;
        public readonly UIElement element;
        public readonly ComputedStyle computedStyle;     
        internal IStyleSystem styleSystem;

        // todo -- remove and replace w/ style
        public HorizontalScrollbarAttachment horizontalScrollbarAttachment => HorizontalScrollbarAttachment.Bottom;
        public VerticalScrollbarAttachment verticalScrollbarAttachment => VerticalScrollbarAttachment.Right;

        private static readonly HashSet<StylePropertyId> s_DefinedMap = new HashSet<StylePropertyId>();

        public UIStyleSet(UIElement element, IStyleSystem styleSystem) {
            this.element = element;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.styleSystem = styleSystem;
            this.computedStyle = new ComputedStyle(this); // todo -- get rid of computed style? use 1 intmap for all styles
            this.appliedStyles = new LightList<StyleEntry>();
            this.styleGroups = new List<UIStyleGroup>();
        }

        public string BaseStyleNames => styleNames;
        public StyleState CurrentState => currentState;

        public List<UIStyleGroup> GetBaseStyles() {
            List<UIStyleGroup> retn = ListPool<UIStyleGroup>.Get();
            for (int i = 0; i < styleGroups.Count; i++) {
                retn.Add(styleGroups[i]);
            }

            return retn;
        }

        public void EnterState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) != 0) {
                return;
            }

            currentState |= state;

            if ((containedStates & state) == 0) {
                return;
            }

            for (int i = 0; i < appliedStyles.Count; i++) {
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

            if ((containedStates & state) == 0) {
                return;
            }

            for (int i = 0; i < appliedStyles.Count; i++) {
                if ((appliedStyles[i].state != state)) {
                    continue;
                }

                UpdateStyleProperties(appliedStyles[i].style);
            }
        }

        public bool HasHoverStyle => (containedStates & StyleState.Hover) != 0;

        public void PlayAnimation(string name) { }

        public void PlayAnimation(StyleAnimation animation) {
            styleSystem.PlayAnimation(this, animation, default(AnimationOptions));
        }

        public bool HandlesOverflow => computedStyle.OverflowX != Overflow.None || computedStyle.OverflowY != Overflow.None;

        public bool HandlesOverflowX => computedStyle.OverflowX != Overflow.None;

        public bool HandlesOverflowY => computedStyle.OverflowY != Overflow.None;

        public bool HasBaseStyles => styleGroups.Count > 0;

        private static string GetBaseStyleNames(UIStyleSet styleSet) {
            string output = string.Empty;
            for (int i = 0; i < styleSet.styleGroups.Count; i++) {
                output += styleSet.styleGroups[i].name;
            }

            return output;
        }

        public void OnAnimationStart() { }

        private void UpdateStyleProperties(UIStyle style) {
            IReadOnlyList<StyleProperty> properties = style.Properties;
            for (int j = 0; j < properties.Count; j++) {
                computedStyle.SetProperty(GetPropertyValueInState(properties[j].propertyId, currentState));
            }
        }

        public void AddStyleGroup(UIStyleGroup group) {
            if (styleGroups.Contains(group)) {
                return;
            }
            // todo -- subscribe to changes?
            styleGroups.Add(group);
            styleNames = GetBaseStyleNames(this);
            if (group.normal != null) AddBaseStyle(group.normal, StyleState.Normal);
            if (group.active != null) AddBaseStyle(group.active, StyleState.Active);
            if (group.inactive != null) AddBaseStyle(group.inactive, StyleState.Inactive);
            if (group.focused != null) AddBaseStyle(group.focused, StyleState.Focused);
            if (group.hover != null) AddBaseStyle(group.hover, StyleState.Hover);
        }

        private void AddBaseStyle(UIStyle style, StyleState state) {
            containedStates |= state;
            appliedStyles.Add(new StyleEntry(style, StyleType.Shared, state, styleGroups.Count));
            SortStyles();
            if (IsInState(state)) {
                UpdateStyleProperties(style);
            }
        }

        // todo remove and replace w/ RemoveStyleGroup
        public void RemoveBaseStyle(UIStyle style, StyleState state = StyleState.Normal) {
            for (int i = 0; i < appliedStyles.Count; i++) {
                if (appliedStyles[i].style == style && state == appliedStyles[i].state) {
                    appliedStyles[i] = appliedStyles[appliedStyles.Count - 1];
                    break;
                }
            }

            appliedStyles.RemoveAt(appliedStyles.Count - 1);
            SortStyles();

            for (int i = 0; i < appliedStyles.Count; i++) {
                containedStates |= appliedStyles[i].state;
            }

            if (IsInState(state)) {
                UpdateStyleProperties(style);
            }
        }

        private void SortStyles() {
            appliedStyles.Sort((a, b) => a.priority > b.priority ? -1 : 1);
        }

        public StyleProperty GetPropertyValue(StylePropertyId propertyId) {
            // can't use ComputedStyle here because this is used to compute that value
            return GetPropertyValueInState(propertyId, currentState);
        }

        public StyleProperty GetPropertyValueInState(StylePropertyId propertyId, StyleState state) {

            for (int i = 0; i < appliedStyles.Count; i++) {
                if ((appliedStyles[i].state & state) == 0) {
                    continue;
                }
                StyleProperty property = appliedStyles[i].style.GetProperty(propertyId);
                if (property.IsDefined) {
                    return property;
                }
            }

            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue);
        }

        
        private UIStyle GetOrCreateInstanceStyle(StyleState state) {
            if (instanceStyle == null) {
                instanceStyle = new UIStyleGroup();
            }

            switch (state) {
                case StyleState.Normal:
                    if (instanceStyle.normal == null) {
                        instanceStyle.normal = new UIStyle();
                        appliedStyles.Add(new StyleEntry(instanceStyle.normal, StyleType.Instance, StyleState.Normal));
                        containedStates |= StyleState.Normal;
                        SortStyles();
                    }
                    return instanceStyle.normal;
                
                case StyleState.Hover:
                    if (instanceStyle.hover == null) {
                        instanceStyle.hover = new UIStyle();
                        appliedStyles.Add(new StyleEntry(instanceStyle.hover, StyleType.Instance, StyleState.Hover));
                        SortStyles();
                        containedStates |= StyleState.Hover;
                    }
                    return instanceStyle.hover;
                
                case StyleState.Active:
                    if (instanceStyle.active == null) {
                        instanceStyle.active = new UIStyle();
                        appliedStyles.Add(new StyleEntry(instanceStyle.active, StyleType.Instance, StyleState.Active));
                        SortStyles();
                        containedStates |= StyleState.Active;
                    }
                    return instanceStyle.active;
                
                case StyleState.Inactive:
                    if (instanceStyle.inactive == null) {
                        instanceStyle.inactive = new UIStyle();
                        appliedStyles.Add(new StyleEntry(instanceStyle.inactive, StyleType.Instance, StyleState.Inactive));
                        SortStyles();
                        containedStates |= StyleState.Inactive;
                    }
                    return instanceStyle.inactive;
                
                case StyleState.Focused:
                    if (instanceStyle.focused == null) {
                        instanceStyle.focused = new UIStyle();
                        appliedStyles.Add(new StyleEntry(instanceStyle.focused, StyleType.Instance, StyleState.Focused));
                        SortStyles();
                        containedStates |= StyleState.Focused;
                    }
                    return instanceStyle.focused;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

        }
        
        internal void Initialize() {
            for (int i = 0; i < appliedStyles.Count; i++) {
                StyleEntry entry = appliedStyles[i];
                containedStates |= entry.state;
                if ((entry.state & currentState) == 0) {
                    continue;
                }

                // todo fix this
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

        private void SetGridTrackSizeProperty(StylePropertyId propertyId, GridTrackSize size, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.SetGridTrackSizeProperty(propertyId, size);
            if ((state & currentState) == 0) {
                return;
            }

            computedStyle.SetProperty(GetPropertyValue(propertyId));
        }

        private UIStyle GetActiveStyleForProperty(StylePropertyId stylePropertyId) {
            for (int i = 0; i < appliedStyles.Count; i++) {
                if ((appliedStyles[i].state & currentState) == 0) {
                    continue;
                }

                if ((appliedStyles[i].style.DefinesProperty(stylePropertyId))) {
                    return appliedStyles[i].style;
                }
            }

            return null;
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

        private void SetObjectProperty(StylePropertyId propertyId, object value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.SetObjectProperty(propertyId, value);
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

        private void SetFloatProperty(StylePropertyId propertyId, float value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.SetFloatProperty(propertyId, value);
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

        private void SetFixedLengthProperty(StylePropertyId propertyId, UIFixedLength fixedLength, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.SetUIFixedLengthProperty(propertyId, fixedLength);
            if ((state & currentState) == 0) {
                return;
            }

            computedStyle.SetProperty(GetPropertyValue(propertyId));
        }

        internal void InitializeScrollbar(VirtualScrollbar scrollbar) {
            UIStyleSet styleSet = new UIStyleSet(scrollbar, styleSystem);
            styleSet.SetBackgroundColor(Color.green, StyleState.Normal);
            styleSet.SetLayoutBehavior(LayoutBehavior.Ignored, StyleState.Normal);
            scrollbar.style = styleSet;
            UIStyleSet handleStyle = new UIStyleSet(scrollbar.handle, styleSystem);
            handleStyle.SetBackgroundColor(Color.magenta, StyleState.Normal);
            handleStyle.SetLayoutBehavior(LayoutBehavior.Ignored, StyleState.Normal);
            scrollbar.handle.style = handleStyle;
        }

        public void SetProperty(StyleProperty property, StyleState state) {
            throw new NotImplementedException();
        }

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