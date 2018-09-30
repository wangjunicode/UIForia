using System;
using System.Collections.Generic;
using System.Diagnostics;
using Src;
using Src.Rendering;
using Src.Systems;
using Src.Util;

namespace Rendering {

    [DebuggerDisplay("id = {element.id} state = {currentState}")]
    public partial class UIStyleSet {

        private StyleState currentState;
        private StyleEntry[] appliedStyles;

        private int baseCounter;
        public readonly UIElement element;

        public ComputedStyle computedStyle;

        private StyleState containedStates;

        public TextStyle ownTextStyle;

        internal IStyleSystem styleSystem;

        public HorizontalScrollbarAttachment horizontalScrollbarAttachment => HorizontalScrollbarAttachment.Bottom;
        public VerticalScrollbarAttachment verticalScrollbarAttachment => VerticalScrollbarAttachment.Right;

        public UIStyleSet(UIElement element, IStyleSystem styleSystem) {
            this.element = element;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.styleSystem = styleSystem;
            this.computedStyle = new ComputedStyle();
            this.ownTextStyle = TextStyle.Unset;
        }

        private string content;

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
            if (appliedStyles == null) return;
            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & state) != 0) {
                    Refresh();
                    return;
                }
            }
            
            // for each property in new state
            // update computed if needed
            // SetProperty();
//            GetInstanceStyle(state);
//            GetBaseStyle(state);

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

            if (appliedStyles == null) return;

            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & state) != 0) {
                    Refresh();
                    return;
                }
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

        public void SetInstanceStyle(UIStyle style, StyleState state = StyleState.Normal) {
            if (appliedStyles == null) {
                appliedStyles = new[] {
                    new StyleEntry(new UIStyle(style), StyleType.Instance, state)
                };
                return;
            }

            for (int i = 0; i < appliedStyles.Length; i++) {
                StyleState target = appliedStyles[i].state & state;
                if ((target == state)) {
                    appliedStyles[i] = new StyleEntry(new UIStyle(style), StyleType.Instance, state);
                    return;
                }
            }

            Array.Resize(ref appliedStyles, appliedStyles.Length + 1);
            appliedStyles[appliedStyles.Length - 1] = new StyleEntry(style, StyleType.Instance, state);
            SortStyles();
            Refresh();
        }

        private void SetInstanceStyleNoCopy(UIStyle style, StyleState state = StyleState.Normal) {
            if (appliedStyles == null) {
                appliedStyles = new StyleEntry[1];
                appliedStyles[0] = new StyleEntry(style, StyleType.Instance, state);
                return;
            }

            for (int i = 0; i < appliedStyles.Length; i++) {
                StyleState target = appliedStyles[i].state & state;
                if ((target == state)) {
                    appliedStyles[i] = new StyleEntry(style, StyleType.Instance, state);
                    return;
                }
            }

            Array.Resize(ref appliedStyles, appliedStyles.Length + 1);
            appliedStyles[appliedStyles.Length - 1] = new StyleEntry(style, StyleType.Instance, state);
            SortStyles();
            Refresh();
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
            if (appliedStyles == null) {
                appliedStyles = new StyleEntry[1];
            }
            else {
                Array.Resize(ref appliedStyles, appliedStyles.Length + 1);
            }

            appliedStyles[appliedStyles.Length - 1] = new StyleEntry(style, StyleType.Shared, state, baseCounter++);
            SortStyles();
            Refresh();
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
            Refresh();
        }

        private void SortStyles() {
            Array.Sort(appliedStyles, (a, b) => a.priority > b.priority ? -1 : 1);
        }

        private UIStyle FindActiveStyle(Func<UIStyle, bool> callback) {
            if (appliedStyles == null) return UIStyle.Default;

            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & currentState) == 0) {
                    continue;
                }

                if (callback(appliedStyles[i].style)) {
                    return appliedStyles[i].style;
                }
            }

            // return default if no matches were found
            return UIStyle.Default;
        }

        private UIStyle FindActiveStyleWithoutDefault(Func<UIStyle, bool> callback) {
            if (appliedStyles == null) return null;

            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & currentState) == 0) {
                    continue;
                }

                if (callback(appliedStyles[i].style)) {
                    return appliedStyles[i].style;
                }
            }

            return null;
        }

        private StyleProperty GetPropertyValueInState(StylePropertyId propertyId, StyleState state) {
            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & currentState) == 0) {
                    continue;
                }

                StyleProperty property = appliedStyles[i].style.FindProperty(propertyId);
                if (property.IsDefined) {
                    return property;
                }
            }

            return StyleProperty.Unset;
        }

        private UIStyle GetStyle(StyleState state) {
            if (appliedStyles == null) return null;

            // only return instance styles
            for (int i = 0; i < appliedStyles.Length; i++) {
                StyleState checkFlag = appliedStyles[i].state;
                UIStyle style = appliedStyles[i].style;
                if ((checkFlag & state) != 0) {
                    return style;
                }
            }

            return null;
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

        // only return instance styles
        private UIStyle GetOrCreateInstanceStyle(StyleState state) {
            if (appliedStyles == null) {
                UIStyle newStyle = new UIStyle();
                SetInstanceStyleNoCopy(newStyle, state);
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

        private static readonly HashSet<StylePropertyId> s_DefinedMap = new HashSet<StylePropertyId>();

        internal void Initialize() {
            
            ComputedStyle.SetDefaults(computedStyle);
            
            for (int i = 0; i < appliedStyles.Length; i++) {
                StyleEntry entry = appliedStyles[i];
                if ((entry.state & currentState) == 0) {
                    continue;
                }

                IReadOnlyList<StyleProperty> properties = entry.style.Properties;
                for (int j = 0; j < properties.Count; j++) {
                    if (!s_DefinedMap.Contains(properties[i].propertyId)) {
                        s_DefinedMap.Add(properties[i].propertyId);
                        computedStyle.SetProperty(properties[i]);
                    }
                }
            }
            
            s_DefinedMap.Clear();
        }

        private static bool IsDefined(ComputedStyle target, StyleProperty property) { }

        private float GetFloatValue(StylePropertyId propertyId, StyleState state) {
            StyleProperty property = GetPropertyValueInState(propertyId, state);
            return property.IsDefined
                ? FloatUtil.DecodeToFloat(property.valuePart0)
                : FloatUtil.UnsetValue;
        }

        private UIMeasurement GetUIMeasurementValue(StylePropertyId propertyId, StyleState state) {
            StyleProperty property = GetPropertyValueInState(propertyId, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }
        
        internal void Refresh() {
            containedStates = StyleState.Normal;

            // todo -- make this suck less

            if (appliedStyles != null) {
                for (int i = 0; i < appliedStyles.Length; i++) {
                    containedStates |= appliedStyles[i].state;
                }
            }

            UIStyle activeFontSizeStyle = FindActiveStyleWithoutDefault((s) => IntUtil.IsDefined(s.textStyle.fontSize));
            UIStyle activeFontColorStyle = FindActiveStyleWithoutDefault((s) => ColorUtil.IsDefined(s.textStyle.color));

            styleSystem.SetFontSize(element, activeFontSizeStyle?.textStyle.fontSize ?? IntUtil.UnsetValue);
            styleSystem.SetFontColor(element, activeFontColorStyle?.textStyle.color ?? ColorUtil.UnsetValue);

            styleSystem.SetPaint(element, paint);
            styleSystem.SetLayout(element, layoutParameters);
            styleSystem.SetMargin(element, margin);
            styleSystem.SetPadding(element, padding);
            styleSystem.SetBorder(element, border);
            styleSystem.SetBorderRadius(element, borderRadius);
            styleSystem.SetDimensions(element, dimensions);
            styleSystem.SetTextStyle(element, textStyle);
            styleSystem.SetAvailableStates(element, containedStates);
            // todo -- change this to a diff with a new computed style object

            /*
             * computed.minWidth = hasMinWidth ? minWidth : defaultMinWidth;
             * for each style definition (sorted by property and by priority)
             *     computed.SetProperty(firstActive.Category, firstActive.Value);
             *     skip to next property
             * 
             */
            // for each entry that was applied before
            // if no longer applied
            // gather property ids
            // for each entry that will be applied
            // for each property 

            StyleState previousState = 0;
            for (int i = 0; i < appliedStyles.Length; i++) {
                bool wasApplied = (appliedStyles[i].state & previousState) != 0;
                if (!wasApplied) {
                    continue;
                }

                IReadOnlyList<StyleProperty> oldProperties = appliedStyles[i].style.Properties;
                for (int j = 0; j < oldProperties.Count; j++) {
                    // add to set
                }
            }

            for (int i = 0; i < appliedStyles.Length; i++) {
                bool active = (appliedStyles[i].state & currentState) != 0;
                if (!active) {
                    continue;
                }
            }
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
        
        private UIStyle UpdateProperty(StylePropertyId stylePropertyId) {
            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & currentState) == 0) {
                    continue;
                }

                StyleProperty property = appliedStyles[i].style.FindProperty(stylePropertyId);
                if (property.IsDefined) {
                    computedStyle.SetProperty(property);
                    return;
                }
            }
            computedStyle.SetProperty(new StyleProperty(stylePropertyId, -123456789, -123456789));
            return null;
        }

    }

}