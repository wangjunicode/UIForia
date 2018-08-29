using System;
using System.Diagnostics;
using Src.Systems;

namespace Rendering {

    [DebuggerDisplay("id = {elementId} state = {currentState}")]
    public partial class UIStyleSet {

        private StyleState currentState;
        private StyleEntry[] appliedStyles;
        private int baseCounter;
        public readonly int elementId;
        private readonly IStyleChangeHandler changeHandler;
        
        public UIStyle activeStyles;

        private StyleState containedStates; 
        
        public UIStyleSet(int elementId, IStyleChangeHandler changeHandler) {
            this.elementId = elementId;
            this.changeHandler = changeHandler;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.activeStyles = new UIStyle();
        }

        public string textContent { get; private set; }
        
        public void EnterState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) != 0) {
                return;
            }
            currentState |= state;
            Refresh();
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
                appliedStyles = new StyleEntry[1];
                appliedStyles[0] = new StyleEntry(new UIStyle(style), StyleType.Instance, state);
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
        }

        private void SortStyles() {
            Array.Sort(appliedStyles, (a, b) => a.priority > b.priority ? -1 : 1);
        }

        private UIStyle FindActiveStyle(Func<UIStyle, bool> callback) {
            if (appliedStyles == null) return UIStyle.Default;

            for (int i = 0; i < appliedStyles.Length; i++) {
                if ((appliedStyles[i].state & currentState) != 0) {
                    if (callback(appliedStyles[i].style)) {
                        return appliedStyles[i].style;
                    }
                }
            }

            // return default if no matches were found
            return UIStyle.Default;
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
        
        // todo -- hide this again
        public void Refresh() {
            containedStates = StyleState.Normal;

            if (appliedStyles != null) {

                for (int i = 0; i < appliedStyles.Length; i++) {
                    containedStates |= appliedStyles[i].state;
                }
                
            }

            changeHandler.SetPaint(elementId, paint);
            changeHandler.SetLayout(elementId, layoutParameters);
            changeHandler.SetConstraints(elementId, constraints);
            changeHandler.SetMargin(elementId, margin);
            changeHandler.SetPadding(elementId, padding);
            changeHandler.SetBorder(elementId, border);
            changeHandler.SetBorderRadius(elementId, borderRadius);
            changeHandler.SetRect(elementId, rect);
            changeHandler.SetText(elementId, textStyle);
            changeHandler.SetAvailableStates(elementId, containedStates);
            
        }

    }

}