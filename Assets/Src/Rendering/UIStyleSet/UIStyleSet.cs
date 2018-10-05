using System;
using System.Collections.Generic;
using System.Diagnostics;
using Src;
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

        public bool HandlesOverflow => computedStyle.OverflowX != Overflow.None || computedStyle.OverflowY != Overflow.None;

        public bool HandlesOverflowX => computedStyle.OverflowX != Overflow.None;

        public bool HandlesOverflowY => computedStyle.OverflowY != Overflow.None;

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

        private StyleProperty GetPropertyValue(StylePropertyId propertyId) {
            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & currentState) == 0) {
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

        private int GetEnumValue(StylePropertyId propertyId, StyleState state) {
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

    }

}