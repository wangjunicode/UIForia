using System;
using Src;
using Src.Layout;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        private StyleEntry[] appliedStyles;
        private StyleState currentState;

        private UIView view;
        private UIElement element;
        private int baseCounter;

        public UIStyleSet(UIElement element, UIView view) {
            currentState = StyleState.Normal;
            this.element = element;
            this.view = view;
        }

        public void EnterState(StyleState type) {
            currentState |= type;
        }

        public void ExitState(StyleState type) {
            if (type == StyleState.Hover) {
                currentState &= ~(StyleState.Hover);
//                view.MarkForRenderStateChange(element);
            }
        }

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

        private UIStyle FindActiveStyle(Func<UIStyle, bool> callback) {
            if (appliedStyles == null) return UIStyle.Default;

            for (int i = 0; i < appliedStyles.Length - 1; i++) {
                if ((appliedStyles[i].state & currentState) != 0) {
                    if (callback(appliedStyles[i].style)) {
                        return appliedStyles[i].style;
                    }
                }
            }

            // return default if no matches were found
            return UIStyle.Default;
        }

        private UIStyle GetOrCreateStyle(StyleState state) {
            // only return instance styles
            UIStyle retn = GetStyle(state);
            if (retn != null) return retn;
            UIStyle style = new UIStyle();
            SetInstanceStyle(style, state);
            return style;
        }

    }

}