using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Animation;
using UIForia.Layout.LayoutTypes;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    [DebuggerDisplay("id = {element.id} state = {currentState}")]
    public partial class UIStyleSet {

        // handful of these could be replaced with a single IntMap, 
        // instance styles, instance states, 
        private string styleNames;
        private UIStyle queryStyle;
        private UIStyle inheritedStyle;
        private StyleState currentState;
        private UIStyleGroup instanceStyle;
        private UIStyleGroup implicitStyle;
        private StyleState containedStates;
        private LightList<StyleEntry> appliedStyles;

        private IntMap<StyleProperty> m_PropertyMap;

        // temp -- replace w/ IntMap & remove computed style
        private List<UIStyleGroup> styleGroups;
        public readonly UIElement element;
        public readonly ComputedStyle computedStyle;
        internal IStyleSystem styleSystem;

        private static readonly HashSet<StylePropertyId> s_DefinedMap = new HashSet<StylePropertyId>();

        public UIStyleSet(UIElement element) {
            this.element = element;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.computedStyle = new ComputedStyle(this);
            this.appliedStyles = new LightList<StyleEntry>();
            this.styleGroups = new List<UIStyleGroup>();
        }

        public string BaseStyleNames => styleNames;
        public StyleState CurrentState => currentState;

        public UIStyleSetStateProxy Hover => new UIStyleSetStateProxy(this, StyleState.Hover);

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

            StyleState oldState = currentState;
            currentState |= state;

            if ((containedStates & state) == 0) {
                return;
            }

            List<StylePropertyId> toUpdate = ListPool<StylePropertyId>.Get();
            for (int i = 0; i < appliedStyles.Count; i++) {
                StyleEntry entry = appliedStyles[i];

                if ((entry.state & oldState) == 0 && (entry.state & state) != 0) {
                    IReadOnlyList<StyleProperty> properties = appliedStyles[i].style.Properties;
                    for (int j = 0; j < properties.Count; j++) {
                        if (!s_DefinedMap.Contains(properties[i].propertyId)) {
                            s_DefinedMap.Add(properties[i].propertyId);
                            toUpdate.Add(properties[i].propertyId);
                        }
                    }
                }
            }

            //LightList<StyleProperty> changeSet = styleSystem.GetChangeSet(element.id);

            for (int i = 0; i < toUpdate.Count; i++) {
                StyleProperty property = GetPropertyValueInState(toUpdate[i], currentState);
                if (property.IsDefined) {
                    StyleProperty currentProperty;
                    if (m_PropertyMap.TryGetValue((int) property.propertyId, out currentProperty)) {
                        if (currentProperty != property) {
                            m_PropertyMap[(int) property.propertyId] = property;
                            styleSystem.SetStyleProperty(element, property);
                        }
                    }
                    else {
                        m_PropertyMap[(int) property.propertyId] = property;
                        styleSystem.SetStyleProperty(element, property);
                    }
                }
                else {
                    styleSystem.SetStyleProperty(element, property);
                }
            }

            // styleSystem.SetChangeSet(element.id, changeSet);

            ListPool<StylePropertyId>.Release(ref toUpdate);
            s_DefinedMap.Clear();
        }

        internal bool SetInheritedStyle(StyleProperty property) {
            if (m_PropertyMap.ContainsKey((int) property.propertyId)) {
                return false;
            }

            StyleProperty current = inheritedStyle.GetProperty(property.propertyId);
            if (current != property) {
                inheritedStyle.SetProperty(current);
                return true;
            }

            return false;
        }

        public bool DidPropertyChange(StylePropertyId property) {
            return false; // styleSystem.GetChangeSet(element.id).DidChange(property);
        }

        public bool IsInState(StyleState state) {
            return (currentState & state) != 0;
        }

        public float Get(StylePropertyId propertyId, float defaultValue) {
            StyleProperty property;
            if (m_PropertyMap.TryGetValue((int) propertyId, out property)) return property.AsFloat;
            return defaultValue;
        }

        public float GetInheritedProperty(StylePropertyId propertyId, float defaultValue) {
            StyleProperty property;
            if (m_PropertyMap.TryGetValue((int) propertyId, out property)) return property.AsFloat;
            if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) propertyId), out property)) return property.AsFloat;
            return defaultValue;
        }

        public void ExitState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) == 0) {
                return;
            }

            currentState &= ~(state);
            currentState |= StyleState.Normal;

            // if any style did apply to state and now doesn't
            // get a list of those styles
            // for each one
            // update
            // 
            if ((containedStates & state) == 0) {
                return;
            }

            for (int i = 0; i < appliedStyles.Count; i++) {
                if ((appliedStyles[i].state != state)) {
                    continue;
                }

                // add each property to defined map
                // run through defined map and apply styles
                // 

                UpdateStyleProperties(appliedStyles[i].style);
            }

            // for each inherited style
            // if !m_DefinedMap.ContainsKey((int)styleId)
            // inheritedStyles[styleId] = FindInheritedProperty(styleId)
            // if value changed -> add to change list        
        }

        public bool HasHoverStyle => (containedStates & StyleState.Hover) != 0;

        public void PlayAnimation(StyleAnimation animation) {
            styleSystem.PlayAnimation(this, animation, default(AnimationOptions));
        }

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

        // I think this won't return normal or inherited styles right now, should it?
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
                instanceStyle.name = "Instance";
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
                        appliedStyles.Add(new StyleEntry(instanceStyle.inactive, StyleType.Instance,
                            StyleState.Inactive));
                        SortStyles();
                        containedStates |= StyleState.Inactive;
                    }

                    return instanceStyle.inactive;

                case StyleState.Focused:
                    if (instanceStyle.focused == null) {
                        instanceStyle.focused = new UIStyle();
                        appliedStyles.Add(new StyleEntry(instanceStyle.focused, StyleType.Instance,
                            StyleState.Focused));
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

        public void SetProperty(StyleProperty property, StyleState state) {
            throw new NotImplementedException();
        }

        // todo -- remove & generate
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

        public string GetPropertySource(StylePropertyId propertyId) {
            if (!computedStyle.IsDefined(propertyId)) {
                return "Default";
            }

            for (int i = 0; i < appliedStyles.Count; i++) {
                if ((currentState & appliedStyles[i].state) == 0) {
                    continue;
                }

                if (appliedStyles[i].style.DefinesProperty(propertyId)) {
                    if (appliedStyles[i].type == StyleType.Instance) {
                        switch (appliedStyles[i].state) {
                            case StyleState.Normal:
                                return "Instance [Normal]";

                            case StyleState.Hover:
                                return "Instance [Hover]";

                            case StyleState.Active:
                                return "Instance [Active]";

                            case StyleState.Inactive:
                                return "Instance [Inactive]";

                            case StyleState.Focused:
                                return "Instance [Focused]";

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    UIStyle style = appliedStyles[i].style;

                    for (int j = 0; j < styleGroups.Count; j++) {
                        UIStyleGroup group = styleGroups[j];

                        if (style == group.normal) {
                            return group.name + " [Normal]";
                        }

                        if (style == group.hover) {
                            return group.name + " [Hover]";
                        }

                        if (style == group.active) {
                            return group.name + " [Active]";
                        }

                        if (style == group.active) {
                            return group.name + " [Inactive]";
                        }
                    }
                }
            }

            return "Unknown";
        }

        public UIStyleGroup GetInstanceStyle() {
            return instanceStyle;
        }

        public void SetPropertyValueInState(StyleProperty property, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            if ((state & currentState) == 0) {
                style.SetProperty(property);
                return;
            }

            StyleProperty oldValue = GetPropertyValue(property.propertyId);

            style.SetProperty(property);

            StyleProperty currentValue = GetPropertyValue(property.propertyId);

            if (oldValue != currentValue) {
                if (currentValue.IsDefined) {
                    m_PropertyMap[(int) property.propertyId] = currentValue;
                }
                else {
                    m_PropertyMap.Remove((int) property.propertyId);
                }

                styleSystem.SetStyleProperty(element, currentValue);
            }
        }

    }

}