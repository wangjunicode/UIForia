using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Animation;
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

        internal IntMap<StyleProperty> m_PropertyMap;

        private List<UIStyleGroup> styleGroups;
        public readonly UIElement element;
        internal IStyleSystem styleSystem;

        private static readonly HashSet<StylePropertyId> s_DefinedMap = new HashSet<StylePropertyId>();

        public UIStyleSet(UIElement element) {
            this.element = element;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.appliedStyles = new LightList<StyleEntry>();
            this.styleGroups = new List<UIStyleGroup>();
            this.m_PropertyMap = new IntMap<StyleProperty>();
            this.inheritedStyle = new UIStyle(); // add to group list
        }

        public string BaseStyleNames => styleNames;
        public StyleState CurrentState => currentState;

        public UIStyleSetStateProxy Hover => new UIStyleSetStateProxy(this, StyleState.Hover);

        public void SetGridItemPlacement(int colStart, int colSpan, int rowStart, int rowSpan, StyleState state) {
            SetGridItemColStart(colStart, state);
            SetGridItemColSpan(colSpan, state);
            SetGridItemRowStart(rowStart, state);
            SetGridItemRowSpan(rowSpan, state);
        }

        public BorderRadius GetBorderRadius(StyleState state) {
            return new BorderRadius(
                GetBorderRadiusTopLeft(state),
                GetBorderRadiusTopRight(state),
                GetBorderRadiusBottomRight(state),
                GetBorderRadiusBottomLeft(state)
            );
        }

        public void SetBorder(FixedLengthRect value, StyleState state) {
            SetBorderTop(value.top, state);
            SetBorderRight(value.right, state);
            SetBorderBottom(value.bottom, state);
            SetBorderLeft(value.left, state);
        }

        public FixedLengthRect GetBorder(StyleState state) {
            return new FixedLengthRect(
                GetBorderTop(state),
                GetBorderRight(state),
                GetBorderBottom(state),
                GetBorderLeft(state)
            );
        }

        public void SetMargin(ContentBoxRect value, StyleState state) {
            SetMarginTop(value.top, state);
            SetMarginRight(value.right, state);
            SetMarginBottom(value.bottom, state);
            SetMarginLeft(value.bottom, state);
        }

        public ContentBoxRect GetMargin(StyleState state) {
            return new ContentBoxRect(
                GetMarginTop(state),
                GetMarginRight(state),
                GetMarginBottom(state),
                GetMarginLeft(state)
            );
        }

        public void SetPadding(FixedLengthRect value, StyleState state) {
            SetPaddingTop(value.top, state);
            SetPaddingRight(value.right, state);
            SetPaddingBottom(value.bottom, state);
            SetPaddingLeft(value.left, state);
        }

        public FixedLengthRect GetPadding(StyleState state) {
            return new FixedLengthRect(
                GetPaddingTop(state),
                GetPaddingRight(state),
                GetPaddingBottom(state),
                GetPaddingLeft(state)
            );
        }

        public void SetTransformPosition(FixedLengthVector position, StyleState state) {
            SetTransformPositionX(position.x, state);
            SetTransformPositionY(position.y, state);
        }

        public void SetBorderRadius(BorderRadius newBorderRadius, StyleState state) {
            SetBorderRadiusBottomLeft(newBorderRadius.bottomLeft, state);
            SetBorderRadiusBottomRight(newBorderRadius.bottomRight, state);
            SetBorderRadiusTopRight(newBorderRadius.topRight, state);
            SetBorderRadiusTopLeft(newBorderRadius.topLeft, state);
        }

        public void SetTransformPosition(Vector2 position, StyleState state) {
            SetTransformPositionX(position.x, state);
            SetTransformPositionY(position.y, state);
        }

        public void SetTransformBehavior(TransformBehavior behavior, StyleState state) {
            SetTransformBehaviorX(behavior, state);
            SetTransformBehaviorY(behavior, state);
        }

        public List<UIStyleGroup> GetBaseStyles() {
            List<UIStyleGroup> retn = ListPool<UIStyleGroup>.Get();
            for (int i = 0; i < styleGroups.Count; i++) {
                retn.Add(styleGroups[i]);
            }

            return retn;
        }

        internal void Initialize() {
            List<StylePropertyId> toUpdate = ListPool<StylePropertyId>.Get();

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
                        toUpdate.Add(properties[j].propertyId);
                        // todo -- set SetProperty(properties[j]);
                    }
                }
            }

            for (int i = 0; i < toUpdate.Count; i++) {
                m_PropertyMap[(int) toUpdate[i]] = GetPropertyValueInState(toUpdate[i], currentState);
            }

            // inheritance?
            ListPool<StylePropertyId>.Release(ref toUpdate);
            s_DefinedMap.Clear();
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

                // if this is a state we had not been in before, mark it's properties for update
                if ((entry.state & oldState) == 0 && (entry.state & state) != 0) {
                    IReadOnlyList<StyleProperty> properties = appliedStyles[i].style.Properties;
                    for (int j = 0; j < properties.Count; j++) {
                        if (!s_DefinedMap.Contains(properties[j].propertyId)) {
                            s_DefinedMap.Add(properties[j].propertyId);
                            toUpdate.Add(properties[j].propertyId);
                        }
                    }
                }
            }

            //LightList<StyleProperty> changeSet = styleSystem.GetChangeSet(element.id);

            // todo batch apply changes
            for (int i = 0; i < toUpdate.Count; i++) {
                StyleProperty property;

                // for each property we are updating
                // if it wasn't set before, set it 

                // if it was set before
                // compare values
                // update if changed

                if (!m_PropertyMap.TryGetValue((int) toUpdate[i], out property)) {
                    property = GetPropertyValueInState(toUpdate[i], currentState);
                    m_PropertyMap[(int) property.propertyId] = property;
                    styleSystem.SetStyleProperty(element, property);
                    continue;
                }

                // will be defined because of the new styles defined it
                StyleProperty currentProperty = GetPropertyValueInState(property.propertyId, currentState);

                if (currentProperty != property) {
                    m_PropertyMap[(int) property.propertyId] = currentProperty;
                    styleSystem.SetStyleProperty(element, currentProperty);
                }
                // if they are the same and was previously inherited, override the inheritance
                else if (StyleUtil.IsInherited(property.propertyId)) {
                    throw new Exception("test this");
                }
            }

            // styleSystem.SetChangeSet(element.id, changeSet);

            ListPool<StylePropertyId>.Release(ref toUpdate);
            s_DefinedMap.Clear();
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

            List<StylePropertyId> toUpdate = ListPool<StylePropertyId>.Get();

            for (int i = 0; i < appliedStyles.Count; i++) {
                if ((appliedStyles[i].state != state)) {
                    continue;
                }

                IReadOnlyList<StyleProperty> properties = appliedStyles[i].style.Properties;
                for (int j = 0; j < properties.Count; j++) {
                    if (!s_DefinedMap.Contains(properties[j].propertyId)) {
                        s_DefinedMap.Add(properties[j].propertyId);
                        toUpdate.Add(properties[j].propertyId);
                    }
                }
            }

            for (int i = 0; i < toUpdate.Count; i++) {
                StyleProperty property;
                // get old value
                // get new value

                // will always be defined
                StyleProperty oldValue = m_PropertyMap[(int) toUpdate[i]];

                if (TryGetPropertyValueInState(toUpdate[i], currentState, out property)) {
                    if (oldValue != property) {
                        m_PropertyMap[(int) property.propertyId] = property;
                        styleSystem.SetStyleProperty(element, property);
                    }
                }
                else {
                    // if was inherited, do that
                    m_PropertyMap.Remove((int) toUpdate[i]);
                }
            }

            // todo handle inherited that are no longer defined

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
            return false; //styleSystem.GetChangeSet(element.id).DidChange(property);
        }

        public bool IsInState(StyleState state) {
            return (currentState & state) != 0;
        }


        public void PlayAnimation(StyleAnimation animation) {
            styleSystem.PlayAnimation(this, animation, default(AnimationOptions));
        }

        public bool HasBaseStyles => styleGroups.Count > 0;
        public float EmSize => 16f;
        public float LineHeightSize => 16f;

        public bool HasBorderRadius =>
            BorderRadiusTopLeft.value > 0 ||
            BorderRadiusBottomLeft.value > 0 ||
            BorderRadiusTopRight.value > 0 ||
            BorderRadiusBottomLeft.value > 0;

        private static string GetBaseStyleNames(UIStyleSet styleSet) {
            string output = string.Empty;
            for (int i = 0; i < styleSet.styleGroups.Count; i++) {
                output += styleSet.styleGroups[i].name;
            }

            return output;
        }

        public BorderRadius BorderRadius => new BorderRadius(BorderRadiusTopLeft, BorderRadiusTopRight, BorderRadiusBottomRight, BorderRadiusBottomLeft);

        public Vector4 ResolvedBorderRadius => new Vector4(
            ResolveHorizontalFixedLength(BorderRadiusTopLeft),
            ResolveHorizontalFixedLength(BorderRadiusTopRight),
            ResolveHorizontalFixedLength(BorderRadiusBottomRight),
            ResolveHorizontalFixedLength(BorderRadiusBottomLeft)
        );

        public Vector4 ResolvedBorder => new Vector4(
            ResolveVerticalFixedLength(BorderTop),
            ResolveHorizontalFixedLength(BorderRight),
            ResolveVerticalFixedLength(BorderBottom),
            ResolveHorizontalFixedLength(BorderLeft)
        );

        // todo -- handle inherited?
        public bool IsDefined(StylePropertyId propertyId) {
            return m_PropertyMap.ContainsKey((int) propertyId);
        }

        // I don't love having this here
        private float ResolveHorizontalFixedLength(UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Pixel:
                    return length.value;

                case UIFixedUnit.Percent:
                    return element.layoutResult.AllocatedWidth * length.value;

                case UIFixedUnit.Em:
                    return EmSize * length.value;

                case UIFixedUnit.ViewportWidth:
                    return 0;

                case UIFixedUnit.ViewportHeight:
                    return 0;

                default:
                    return 0;
            }
        }

        // I don't love having this here
        private float ResolveVerticalFixedLength(UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Pixel:
                    return length.value;

                case UIFixedUnit.Percent:
                    return element.layoutResult.AllocatedHeight * length.value;

                case UIFixedUnit.Em:
                    return EmSize * length.value;

                case UIFixedUnit.ViewportWidth:
                    return 0;

                case UIFixedUnit.ViewportHeight:
                    return 0;

                default:
                    return 0;
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
            StyleEntry newEntry = new StyleEntry(style, StyleType.Shared, state, styleGroups.Count);
            if (!IsInState(state)) {
                appliedStyles.Add(newEntry);
                SortStyles();
                return;
            }

            // todo -- use change list
            IReadOnlyList<StyleProperty> properties = style.Properties;

            for (int i = 0; i < properties.Count; i++) {
                StyleProperty property = properties[i];

                StyleEntry entry;

                if (TryGetActiveStyleForProperty(properties[i].propertyId, out entry)) {
                    if (entry.priority < newEntry.priority) {
                        m_PropertyMap[(int) property.propertyId] = property;
                        styleSystem.SetStyleProperty(element, property);
                    }
                }
                else {
                    if (StyleUtil.IsInherited(property.propertyId)) {
                        m_PropertyMap.Remove(BitUtil.SetHighLowBits(1, (int) property.propertyId));
                    }

                    m_PropertyMap[(int) property.propertyId] = property;
                    styleSystem.SetStyleProperty(element, property);
                }
            }

            appliedStyles.Add(newEntry);
            SortStyles();
        }

        private bool TryGetActiveStyleForProperty(StylePropertyId propertyId, out StyleEntry retn) {
            for (int i = 0; i < appliedStyles.Count; i++) {
                if ((appliedStyles[i].state & currentState) != 0 && appliedStyles[i].style.DefinesProperty(propertyId)) {
                    retn = appliedStyles[i];
                    return true;
                }
            }

            retn = default(StyleEntry);
            return false;
        }

        // todo remove and replace w/ RemoveStyleGroup
        public void RemoveBaseStyle(UIStyle style, StyleState state = StyleState.Normal) {
            StyleEntry toRemove = default(StyleEntry);
            int index = -1;
            for (int i = 0; i < appliedStyles.Count; i++) {
                if (appliedStyles[i].style == style && state == appliedStyles[i].state) {
                    toRemove = appliedStyles[i];
                    appliedStyles[i] = appliedStyles[appliedStyles.Count - 1];
                    index = i;
                    break;
                }
            }
           
            if (index == -1) {
                return;
            }
            
            appliedStyles.RemoveAt(appliedStyles.Count - 1);
            SortStyles();

            for (int i = 0; i < appliedStyles.Count; i++) {
                containedStates |= appliedStyles[i].state;
            }

            if (!IsInState(state)) {
                return;
            }

            List<StylePropertyId> toUpdate = ListPool<StylePropertyId>.Get();

            IReadOnlyList<StyleProperty> properties = toRemove.style.Properties;
            for (int i = 0; i < properties.Count; i++) {
                toUpdate.Add(properties[i].propertyId);
            }

            for (int i = 0; i < toUpdate.Count; i++) {
                StyleProperty property;
                StyleProperty oldValue = m_PropertyMap[(int) toUpdate[i]];

                if (TryGetPropertyValueInState(toUpdate[i], currentState, out property)) {
                    if (oldValue != property) {
                        m_PropertyMap[(int) property.propertyId] = property;
                        styleSystem.SetStyleProperty(element, property);
                    }
                }
                else {
                    // todo -- if was inherited, do that
                    m_PropertyMap.Remove((int) toUpdate[i]);
                }
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

                StyleProperty property;
                if (appliedStyles[i].style.TryGetProperty(propertyId, out property)) {
                    return property;
                }
            }

            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue, FloatUtil.UnsetValue, null);
        }

        // I think this won't return normal or inherited styles right now, should it?
        public bool TryGetPropertyValueInState(StylePropertyId propertyId, StyleState state, out StyleProperty property) {
            for (int i = 0; i < appliedStyles.Count; i++) {
                if ((appliedStyles[i].state & state) == 0) {
                    continue;
                }

                if (appliedStyles[i].style.TryGetProperty(propertyId, out property)) {
                    return true;
                }
            }

            property = default(StyleProperty);
            return false;
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

        public string GetPropertySource(StylePropertyId propertyId) {
            if (!IsDefined(propertyId)) {
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

        public void SetProperty(StyleProperty property, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            if ((state & currentState) == 0) {
                style.SetProperty(property);
                return;
            }

            // todo -- faster to find style that currently defines the property & compare priorities

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

        public void SetAnimatedProperty(StyleProperty p0) {
            throw new NotImplementedException();
        }
       

    }

}