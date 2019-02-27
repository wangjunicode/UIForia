using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Animation;
using UIForia.Compilers.Style;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Rendering {

    [DebuggerDisplay("id = {element.id} state = {currentState}")]
    public partial class UIStyleSet {

        public readonly UIElement element;

        private string styleNames;

        // private UIStyle queryStyle;
        private StyleState currentState;
        private UIStyleGroup instanceStyle;
        private StyleState containedStates;
        private readonly LightList<StyleEntry> availableStyles;
        private readonly LightList<UIStyleGroupContainer> styleGroupContainers;
        internal readonly IntMap<StyleProperty> propertyMap;
        internal IStyleSystem styleSystem; // needed? can reference via element
        private bool hasAttributeStyles;

        // idea -- for styles are inactive, sort them to the back of the available styles list,
        // then we have to look though less of an array (also track a count for how many styles are active)

        public UIStyleSet(UIElement element) {
            this.element = element;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.availableStyles = new LightList<StyleEntry>();
            this.styleGroupContainers = new LightList<UIStyleGroupContainer>();
            this.propertyMap = new IntMap<StyleProperty>();
            this.hasAttributeStyles = false;
        }

        public string BaseStyleNames => styleNames;
        public StyleState CurrentState => currentState;

        public UIStyleSetStateProxy Normal => new UIStyleSetStateProxy(this, StyleState.Normal);
        public UIStyleSetStateProxy Hover => new UIStyleSetStateProxy(this, StyleState.Hover);
        public UIStyleSetStateProxy Focus => new UIStyleSetStateProxy(this, StyleState.Focused);
        public UIStyleSetStateProxy Active => new UIStyleSetStateProxy(this, StyleState.Active);

        public List<UIStyleGroupContainer> GetBaseStyles() {
            List<UIStyleGroupContainer> retn = ListPool<UIStyleGroupContainer>.Get();
            for (int i = 0; i < styleGroupContainers.Count; i++) {
                retn.Add(styleGroupContainers[i]);
            }

            return retn;
        }

        // manual contains check to avoid boxing in the list implementation
        private static bool ListContainsStyleProperty(LightList<StylePropertyId> list, StylePropertyId target) {
            int arraySize = list.Count;
            StylePropertyId[] array = list.Array;
            for (int j = 0; j < arraySize; j++) {
                if (array[j] == target) {
                    return true;
                }
            }

            return false;
        }

        private void CreateStyleEntry(LightList<StylePropertyId> toUpdate, UIStyleGroup group, UIStyle style, StyleType styleType, StyleState styleState, int ruleCount) {
            if (style == null) return;

            containedStates |= styleState;

            if ((currentState & styleState) != 0) {
                AddMissingProperties(toUpdate, style.m_StyleProperties);
            }

            availableStyles.Add(new StyleEntry(group, style, styleType, styleState, availableStyles.Count, ruleCount));
        }

        internal void ClearPropertyMap() {
            // when clearing the property map we want to retain the styles that we have inherited from elsewhere
            // to do this, we need to read in the inherited values, store them, clear the map, then write the values back
            LightList<StyleProperty> inherited = LightListPool<StyleProperty>.Get();
            inherited.EnsureCapacity(StyleUtil.InheritedProperties.Count);
            StyleProperty[] inheritedArray = inherited.Array;
            for (int i = 0; i < StyleUtil.InheritedProperties.Count; i++) {
                int key = BitUtil.SetHighLowBits(1, (int) StyleUtil.InheritedProperties[i]);
                if (propertyMap.TryGetValue(key, out StyleProperty inheritedValue)) {
                    inherited.AddUnchecked(inheritedValue);
                }
            }

            propertyMap.Clear();
            // re-apply values
            for (int i = 0; i < inherited.Count; i++) {
                int key = BitUtil.SetHighLowBits(1, (int) inheritedArray[i].propertyId);
                propertyMap.Add(key, inheritedArray[i]);
            }

            LightListPool<StyleProperty>.Release(ref inherited);
        }

        internal void SetStyleGroups(IList<UIStyleGroupContainer> baseStyles) {
            if (styleGroupContainers.Count > 0) {
                Debug.LogWarning("You should not initialize UIStyleSets twice!!");
                return;
            }

            containedStates = 0;  
            hasAttributeStyles = false;
            styleGroupContainers.Clear();
            ClearPropertyMap();
            availableStyles.Clear();

            LightList<StylePropertyId> toUpdate = LightListPool<StylePropertyId>.Get();
            styleGroupContainers.EnsureCapacity(baseStyles.Count);

            for (int i = 0; i < baseStyles.Count; i++) {
                UIStyleGroupContainer groupContainer = baseStyles[i];

                styleGroupContainers.AddUnchecked(baseStyles[i]);

                for (int j = 0; j < groupContainer.groups.Count; j++) {
                    UIStyleGroup group = groupContainer.groups[j];

                    if (group.HasAttributeRule) {
                        hasAttributeStyles = true;
                    }

                    if (group.rule == null || group.rule != null & group.rule.IsApplicableTo(element)) {
                        int ruleCount = group.CountRules();
                        CreateStyleEntry(toUpdate, group, group.normal, groupContainer.styleType, StyleState.Normal, ruleCount);
                        CreateStyleEntry(toUpdate, group, group.hover, groupContainer.styleType, StyleState.Hover, ruleCount);
                        CreateStyleEntry(toUpdate, group, group.focused, groupContainer.styleType, StyleState.Focused, ruleCount);
                        CreateStyleEntry(toUpdate, group, group.active, groupContainer.styleType, StyleState.Active, ruleCount);
                    }
                }
            }


            SortStyles();

            UpdatePropertyMap(toUpdate);

            LightListPool<StylePropertyId>.Release(ref toUpdate);
        }

        private void AddStyleGroups(LightList<StylePropertyId> toUpdate, UIStyleGroupContainer container) {
            styleGroupContainers.Add(container);

            if (container.hasAttributeStyles) {
                hasAttributeStyles = true;
            }

            for (int j = 0; j < container.groups.Count; j++) {
                UIStyleGroup group = container.groups[j];

                if (group.rule == null || group.rule != null && group.rule.IsApplicableTo(element)) {
                    int ruleCount = group.CountRules();
                    CreateStyleEntry(toUpdate, group, group.normal, container.styleType, StyleState.Normal, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.hover, container.styleType, StyleState.Hover, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.focused, container.styleType, StyleState.Focused, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.active, container.styleType, StyleState.Active, ruleCount);
                }
            }
        }

        private static void AddMissingProperties(LightList<StylePropertyId> toUpdate, LightList<StyleProperty> source) {
            int count = source.Count;
            StyleProperty[] properties = source.Array;
            for (int i = 0; i < count; i++) {
                StylePropertyId propertyId = properties[i].propertyId;
                if (!ListContainsStyleProperty(toUpdate, propertyId)) {
                    toUpdate.Add(propertyId);
                }
            }
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

            LightList<StylePropertyId> toUpdate = LightListPool<StylePropertyId>.Get();

            StyleEntry[] styleEntries = availableStyles.Array;
            for (int i = 0; i < availableStyles.Count; i++) {
                StyleEntry entry = styleEntries[i];

                // if this is a state we had not been in before, mark it's properties for update
                if ((entry.state & oldState) == 0 && (entry.state & state) != 0) {
                    AddMissingProperties(toUpdate, entry.style.m_StyleProperties);
                }
            }

            UpdatePropertyMap(toUpdate);

            LightListPool<StylePropertyId>.Release(ref toUpdate);
        }

        public void ExitState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) == 0) {
                return;
            }

            StyleState oldState = currentState;
            currentState &= ~(state);
            currentState |= StyleState.Normal;

            if ((containedStates & state) == 0) {
                return;
            }

            LightList<StylePropertyId> toUpdate = LightListPool<StylePropertyId>.Get();

            StyleEntry[] styleEntries = availableStyles.Array;
            for (int i = 0; i < availableStyles.Count; i++) {
                StyleEntry entry = styleEntries[i];

                // if this a state we were in that is now invalid, mark it's properties for update
                if ((entry.state & oldState) != 0 && (entry.state & state) == 0) {
                    AddMissingProperties(toUpdate, entry.style.m_StyleProperties);
                }
            }

            UpdatePropertyMap(toUpdate);

            LightListPool<StylePropertyId>.Release(ref toUpdate);
        }

        internal bool SetInheritedStyle(StyleProperty property) {
            if (propertyMap.ContainsKey((int) property.propertyId)) {
                return false;
            }

            int key = BitUtil.SetHighLowBits(1, (int) property.propertyId);
            StyleProperty current;
            if (propertyMap.TryGetValue(key, out current)) {
                if (current != property) {
                    propertyMap[key] = property;
                    return true;
                }

                return false;
            }
            else {
                propertyMap[key] = property;
                return true;
            }
        }

        public bool IsInState(StyleState state) {
            return (currentState & state) != 0;
        }

        public void PlayAnimation(StyleAnimation animation) {
            styleSystem.PlayAnimation(this, animation, default(AnimationOptions));
        }

        public bool HasBaseStyles => styleGroupContainers.Count > 0;
        public float EmSize => 16f; // todo -- wrong
        public float LineHeightSize => 16f; // todo -- wrong

        public bool HasBorderRadius =>
            BorderRadiusTopLeft.value > 0 ||
            BorderRadiusBottomLeft.value > 0 ||
            BorderRadiusTopRight.value > 0 ||
            BorderRadiusBottomLeft.value > 0;

        private static string GetBaseStyleNames(UIStyleSet styleSet) {
            string output = string.Empty;

            for (int i = 0; i < styleSet.styleGroupContainers.Count; i++) {
                output += styleSet.styleGroupContainers[i].name;
                if (i != styleSet.styleGroupContainers.Count - 1) {
                    output += ", ";
                }
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
            return propertyMap.ContainsKey((int) propertyId);
        }

        // I don't love having this here, make accessible on layout result
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

        internal void AddStyleGroupContainer(UIStyleGroupContainer container) {
            if (styleGroupContainers.Contains(container)) {
                return;
            }

            LightList<StylePropertyId> toUpdate = LightListPool<StylePropertyId>.Get();

            AddStyleGroups(toUpdate, container);

            SortStyles();

            UpdatePropertyMap(toUpdate);

            LightListPool<StylePropertyId>.Release(ref toUpdate);
        }

        public void RemoveStyleGroupContainer(UIStyleGroupContainer container) {
            if (!styleGroupContainers.Contains(container)) {
                return;
            }

            styleGroupContainers.Remove(container);

            LightList<StylePropertyId> toUpdate = LightListPool<StylePropertyId>.Get();

            for (int i = 0; i < container.groups.Count; i++) {
                UIStyleGroup group = container.groups[i];

                for (int j = 0; j < availableStyles.Count; j++) {
                    if (availableStyles[j].sourceGroup == group) {
                        if ((availableStyles[j].state & currentState) != 0) {
                            AddMissingProperties(toUpdate, availableStyles[j].style.m_StyleProperties);
                        }

                        availableStyles.RemoveAt(j--);
                    }
                }
            }

            for (int i = 0; i < styleGroupContainers.Count; i++) {
                hasAttributeStyles = hasAttributeStyles || styleGroupContainers[i].hasAttributeStyles;
            }

            for (int i = 0; i < availableStyles.Count; i++) {
                containedStates |= availableStyles[i].state;
            }

            UpdatePropertyMap(toUpdate);

            // todo -- handle inheritance, probably done in the style system and not here

            LightListPool<StylePropertyId>.Release(ref toUpdate);
        }

        private void SortStyles() {
            availableStyles.Sort((a, b) => a.priority < b.priority ? 1 : -1);
        }

        public StyleProperty GetPropertyValue(StylePropertyId propertyId) {
            // can't use ComputedStyle here because this is used to compute that value
            return GetPropertyValueInState(propertyId, currentState);
        }

        // I think this won't return normal or inherited styles right now, should it?
        public StyleProperty GetPropertyValueInState(StylePropertyId propertyId, StyleState state) {
            for (int i = 0; i < availableStyles.Count; i++) {
                if ((availableStyles[i].state & state) == 0) {
                    continue;
                }

                StyleProperty property;
                if (availableStyles[i].style.TryGetProperty(propertyId, out property)) {
                    return property;
                }
            }

            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue, FloatUtil.UnsetValue, null);
        }

        // I think this won't return normal or inherited styles right now, should it?
        public bool TryGetPropertyValueInState(StylePropertyId propertyId, StyleState state, out StyleProperty property) {
            for (int i = 0; i < availableStyles.Count; i++) {
                if ((availableStyles[i].state & state) == 0) {
                    continue;
                }

                if (availableStyles[i].style.TryGetProperty(propertyId, out property)) {
                    return true;
                }
            }

            property = default(StyleProperty);
            return false;
        }

        private UIStyle GetOrCreateInstanceStyle(StyleState state) {
            if (instanceStyle == null) {
                instanceStyle = new UIStyleGroup() {
                    name = "Instance",
                    styleType = StyleType.Instance
                };
            }


            switch (state) {
                case StyleState.Normal:
                    if (instanceStyle.normal == null) {
                        instanceStyle.normal = new UIStyle();
                        availableStyles.Add(new StyleEntry(instanceStyle, instanceStyle.normal, StyleType.Instance, StyleState.Normal, byte.MaxValue, byte.MaxValue));
                        containedStates |= StyleState.Normal;
                        SortStyles();
                    }

                    return instanceStyle.normal;

                case StyleState.Hover:
                    if (instanceStyle.hover == null) {
                        instanceStyle.hover = new UIStyle();
                        availableStyles.Add(new StyleEntry(instanceStyle, instanceStyle.hover, StyleType.Instance, StyleState.Hover, byte.MaxValue, byte.MaxValue));
                        containedStates |= StyleState.Hover;
                        SortStyles();
                    }

                    return instanceStyle.hover;

                case StyleState.Active:
                    if (instanceStyle.active == null) {
                        instanceStyle.active = new UIStyle();
                        availableStyles.Add(new StyleEntry(instanceStyle, instanceStyle.active, StyleType.Instance, StyleState.Active, byte.MaxValue, byte.MaxValue));
                        containedStates |= StyleState.Active;
                        SortStyles();
                    }

                    return instanceStyle.active;

                case StyleState.Focused:
                    if (instanceStyle.focused == null) {
                        instanceStyle.focused = new UIStyle();
                        availableStyles.Add(new StyleEntry(instanceStyle, instanceStyle.focused, StyleType.Instance, StyleState.Focused, byte.MaxValue, byte.MaxValue));
                        containedStates |= StyleState.Focused;
                        SortStyles();
                    }

                    return instanceStyle.focused;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private UIStyleGroupContainer FindContainerForGroup(UIStyleGroup group) {
            for (int i = 0; i < styleGroupContainers.Count; i++) {
                for (int j = 0; j < styleGroupContainers[i].groups.Count; i++) {
                    if (styleGroupContainers[i].groups[j] == group) {
                        return styleGroupContainers[i];
                    }
                }
            }

            return null;
        }

        public string GetPropertySource(StylePropertyId propertyId) {
            if (!IsDefined(propertyId)) {
                return "Default";
            }

            for (int i = 0; i < availableStyles.Count; i++) {
                if ((currentState & availableStyles[i].state) == 0) {
                    continue;
                }

                if (availableStyles[i].style.DefinesProperty(propertyId)) {
                    if (availableStyles[i].type == StyleType.Instance) {
                        switch (availableStyles[i].state) {
                            case StyleState.Normal:
                                return "Instance [Normal]";

                            case StyleState.Hover:
                                return "Instance [Hover]";

                            case StyleState.Active:
                                return "Instance [Active]";

                            case StyleState.Focused:
                                return "Instance [Focused]";

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    if (availableStyles[i].type == StyleType.Shared) {
                        UIStyleGroupContainer container = FindContainerForGroup(availableStyles[i].sourceGroup);
                        string containerName = container?.name ?? "Unknown";
                        switch (availableStyles[i].state) {
                            case StyleState.Normal:
                                return $"{containerName} [Normal]";
                            case StyleState.Active:
                                return $"{containerName} [Active]";
                            case StyleState.Hover:
                                return $"{containerName} [Hover]";
                            case StyleState.Focused:
                                return $"{containerName} [Focused]";
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    switch (availableStyles[i].state) {
                        case StyleState.Normal:
                            return $"<{element.GetDisplayName()}> [Normal]";
                        case StyleState.Active:
                            return $"<{element.GetDisplayName()}> [Active]";
                        case StyleState.Hover:
                            return $"<{element.GetDisplayName()}> [Hover]";
                        case StyleState.Focused:
                            return $"<{element.GetDisplayName()}> [Focused]";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return "Unknown";
        }

        public void SetProperty(StyleProperty property, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            if ((state & currentState) == 0) {
                style.SetProperty(property);
                return;
            }

            StyleProperty oldValue = GetPropertyValue(property.propertyId);

            style.SetProperty(property);

            StyleProperty currentValue;
            if (TryGetPropertyValueInState(property.propertyId, currentState, out currentValue)) {
                if (oldValue != currentValue) {
                    propertyMap[(int) property.propertyId] = currentValue;
                    styleSystem.SetStyleProperty(element, currentValue);
                }
            }
            else {
                propertyMap.Remove((int) property.propertyId);
                styleSystem.SetStyleProperty(element, property);
            }
        }

        public void SetAnimatedProperty(StyleProperty property) {
            if (StyleUtil.CanAnimate(property.propertyId)) {
                SetProperty(property, StyleState.Normal); // todo -- need another priority group for this
            }
        }

        private int FindNextStyleEntryForGroup(UIStyleGroup group, int startIdx) {
            StyleEntry[] entries = availableStyles.Array;
            for (int i = startIdx; i < availableStyles.Count; i++) {
                if (entries[i].sourceGroup == group) return i;
            }

            return -1;
        }

        public void UpdateApplicableAttributeRules(string attributeName, string previousValue) {
            if (!hasAttributeStyles) return;

            int containerCount = styleGroupContainers.Count;
            UIStyleGroupContainer[] containers = styleGroupContainers.Array;

            LightList<StylePropertyId> toUpdate = LightListPool<StylePropertyId>.Get();

            for (int i = 0; i < containerCount; i++) {
                IReadOnlyList<UIStyleGroup> groups = containers[i].groups;
                for (int j = 0; j < groups.Count; j++) {
                    UIStyleGroup group = groups[j];

                    if (!group.HasAttributeRule) {
                        continue;
                    }

                    bool isApplicable = group.rule.IsApplicableTo(element);
                    // if the rule no longer applies, remove the entries from this group from the list
                    // if we had properties from this group that were active, add them to our update list to be recomputed

                    if (!isApplicable) {
                        int nextIdx = 0;
                        while (true) {
                            nextIdx = FindNextStyleEntryForGroup(group, nextIdx);

                            if (nextIdx >= 0) {
                                bool isActive = (availableStyles[nextIdx].state & currentState) != 0;

                                if (isActive) {
                                    AddMissingProperties(toUpdate, availableStyles[nextIdx].style.m_StyleProperties);
                                }

                                availableStyles.RemoveAt(nextIdx);
                                continue;
                            }

                            break;
                        }
                    }
                    else {
                        // if currently active, check if it we have any styles from this group 
                        // if we do then we don't need to do anything
                        // if we don't then we need to create style entries 
                        int nextIdx = FindNextStyleEntryForGroup(group, 0);
                        if (nextIdx == -1) {
                            int ruleCount = group.CountRules();
                            CreateStyleEntry(toUpdate, group, group.normal, group.styleType, StyleState.Normal, ruleCount);
                            CreateStyleEntry(toUpdate, group, group.hover, group.styleType, StyleState.Hover, ruleCount);
                            CreateStyleEntry(toUpdate, group, group.focused, group.styleType, StyleState.Focused, ruleCount);
                            CreateStyleEntry(toUpdate, group, group.active, group.styleType, StyleState.Active, ruleCount);
                        }
                    }
                }
            }

            SortStyles();

            UpdatePropertyMap(toUpdate);

            LightListPool<StylePropertyId>.Release(ref toUpdate);
        }

        private void UpdatePropertyMap(LightList<StylePropertyId> toUpdate) {
            StylePropertyId[] propertyIdArray = toUpdate.Array;

            for (int i = 0; i < toUpdate.Count; i++) {
                StylePropertyId propertyId = propertyIdArray[i];

                StyleProperty oldValue = propertyMap[(int) propertyId];
                if (TryGetPropertyValueInState(propertyId, currentState, out StyleProperty property)) {
                    if (oldValue != property) {
                        propertyMap[(int) property.propertyId] = property;
                        styleSystem.SetStyleProperty(element, property);
                    }
                }
                else {
                    propertyMap.Remove((int) propertyId);
                }
            }

            //styleSystem.SetStyleProperties(element, toUpdate);
        }

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

        public void SetTransformPosition(TransformOffsetPair position, StyleState state) {
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

    }

}