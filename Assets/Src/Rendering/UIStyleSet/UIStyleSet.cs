using System;
using System.Diagnostics;
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
        private readonly IStyleChangeHandler changeHandler;

        public UIStyle computedStyle;

        private StyleState containedStates;

        public TextStyle ownTextStyle;

        private StyleSystem styleSystem;

        public UIStyleSet(UIElement element, IStyleChangeHandler changeHandler, StyleSystem styleSystem) {
            this.element = element;
            this.changeHandler = changeHandler;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.styleSystem = styleSystem;
            this.computedStyle = new UIStyle();
            this.ownTextStyle = TextStyle.Unset;
        }

        private string content;

        public string textContent {
            get { return content; }
            set {
                switch (whiteSpace) {
                    case WhitespaceMode.Unset:
                        content = value;
                        break;
                    case WhitespaceMode.Wrap:
                        content = WhitespaceProcessor.ProcessWrap(value);
                        break;
                    case WhitespaceMode.NoWrap:
                        content = value;
                        break;
                    case WhitespaceMode.Preserve:
                        content = value;
                        break;
                    case WhitespaceMode.PreserveWrap:
                        content = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void EnterState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) != 0) {
                return;
            }

            currentState |= state;
            Refresh();
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
            Refresh();
        }

        public bool HasHoverStyle => (containedStates & StyleState.Hover) != 0;

        public UIStyleProxy hover {
            get { return new UIStyleProxy(this, StyleState.Hover); }
            // ReSharper disable once ValueParameterNotUsed
            set { SetHoverStyle(UIStyleProxy.hack); }
        }

        public UIStyleProxy active {
            get { return new UIStyleProxy(this, StyleState.Active); }
            // ReSharper disable once ValueParameterNotUsed
            set { SetActiveStyle(UIStyleProxy.hack); }
        }

        public UIStyleProxy focused {
            get { return new UIStyleProxy(this, StyleState.Focused); }
            // ReSharper disable once ValueParameterNotUsed
            set { SetFocusedStyle(UIStyleProxy.hack); }
        }

        public UIStyleProxy disabled {
            get { return new UIStyleProxy(this, StyleState.Disabled); }
            // ReSharper disable once ValueParameterNotUsed
            set { SetDisabledStyle(UIStyleProxy.hack); }
        }

        public bool HandlesOverflow => computedStyle.overflowX != Overflow.None || computedStyle.overflowY != Overflow.None;

        public bool HandlesOverflowX => computedStyle.overflowX != Overflow.None;

        public bool HandlesOverflowY => computedStyle.overflowY != Overflow.None;

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

        public void AddBaseStyle(UIStyle style, StyleState state = StyleState.Normal) {
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

        private UIStyle GetStyle(StyleState state) {
            if (appliedStyles == null) return UIStyle.Default;

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

        // only return instance styles
        private UIStyle GetOrCreateStyle(StyleState state) {
            if (appliedStyles == null) {
                UIStyle newStyle = new UIStyle();
                SetInstanceStyleNoCopy(newStyle, state);
                return newStyle;
            }

            UIStyle retn = GetStyle(state);
            if (retn != null && retn != UIStyle.Default) {
                return retn;
            }

            UIStyle style = new UIStyle();
            SetInstanceStyle(style, state);
            return style;
        }

        internal void Refresh() {
            containedStates = StyleState.Normal;

            if (appliedStyles != null) {
                for (int i = 0; i < appliedStyles.Length; i++) {
                    containedStates |= appliedStyles[i].state;
                }
            }

            UIStyle activeFontSizeStyle = FindActiveStyleWithoutDefault((s) => IntUtil.IsDefined(s.textStyle.fontSize));
            UIStyle activeFontColorStyle = FindActiveStyleWithoutDefault((s) => ColorUtil.IsDefined(s.textStyle.color));

            styleSystem.SetFontSize(element, activeFontSizeStyle?.textStyle.fontSize ?? IntUtil.UnsetValue);
            styleSystem.SetFontColor(element, activeFontColorStyle?.textStyle.color ?? ColorUtil.UnsetValue);

            changeHandler.SetPaint(element, paint);
            changeHandler.SetLayout(element, layoutParameters);
            changeHandler.SetConstraints(element, constraints);
            changeHandler.SetMargin(element, margin);
            changeHandler.SetPadding(element, padding);
            changeHandler.SetBorder(element, border);
            changeHandler.SetBorderRadius(element, borderRadius);
            changeHandler.SetDimensions(element, dimensions);
            changeHandler.SetTextStyle(element, textStyle);
            changeHandler.SetAvailableStates(element, containedStates);
        }

    }

}