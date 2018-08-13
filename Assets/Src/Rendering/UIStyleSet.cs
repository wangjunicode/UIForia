using System;
using UnityEngine;

namespace Rendering {

    public class UIStyleSet {

        private StyleFlagPair[] stylePairs;
        private StyleStateType currentStateType;

        private UIView view;
        private UIElement element;

        public UIStyleSet(UIElement element, UIView view) {
            this.element = element;
            this.view = view;
            stylePairs = new StyleFlagPair[1];
            stylePairs[0] = new StyleFlagPair();
        }

        public UIStyleProxy hover {
            get { return new UIStyleProxy(this, StyleStateType.Hover); }
            // ReSharper disable once ValueParameterNotUsed
            set { SetHoverStyle(UIStyleProxy.hack); }
        }

        public UIStyleProxy active {
            get { return new UIStyleProxy(this, StyleStateType.Active); }
            // ReSharper disable once ValueParameterNotUsed
            set { SetActiveStyle(UIStyleProxy.hack); }
        }

        public UIStyleProxy focused {
            get { return new UIStyleProxy(this, StyleStateType.Focused); }
            // ReSharper disable once ValueParameterNotUsed
            set { SetFocusedStyle(UIStyleProxy.hack); }
        }

        public UIStyleProxy disabled {
            get { return new UIStyleProxy(this, StyleStateType.Disabled); }
            // ReSharper disable once ValueParameterNotUsed
            set { SetDisabledStyle(UIStyleProxy.hack); }
        }

        public void SetActiveStyle(UIStyle style) {
            SetInstanceStyle(style, StyleStateType.InstanceActive);
        }

        public void SetFocusedStyle(UIStyle style) {
            SetInstanceStyle(style, StyleStateType.InstanceFocused);
        }

        public void SetHoverStyle(UIStyle style) {
            SetInstanceStyle(style, StyleStateType.InstanceHover);
        }

        public void SetDisabledStyle(UIStyle style) {
            SetInstanceStyle(style, StyleStateType.InstanceDisabled);
        }

        private void SetInstanceStyle(UIStyle style, StyleStateType state) {
            for (int i = 0; i < stylePairs.Length; i++) {
                StyleStateType target = stylePairs[i].checkFlag & state;
                if ((target == state)) {
                    stylePairs[i] = new StyleFlagPair(style, state);
                    stylePairs[i].style.onChange -= OnStyleChanged;
                    style.onChange += OnStyleChanged; // todo only if new
                    return;
                }
            }
            style.onChange += OnStyleChanged;
            Array.Resize(ref stylePairs, stylePairs.Length + 1);
            stylePairs[stylePairs.Length - 1] = new StyleFlagPair(style, state);
            Array.Sort(stylePairs, (a, b) => {
                return (int) a.checkFlag > (int) b.checkFlag ? 1 : -1;
            });
        }

        public void AddBaseStyle(UIStyle style, StyleStateType stateType = StyleStateType.Normal) { }

        public int baseStyleCount {
            get {
                int retn = 0;
                for (int i = 0; i < stylePairs.Length; i++) {
                    if ((stylePairs[i].checkFlag & StyleStateType.Base) != 0) {
                        retn++;
                    }
                }
                return retn;
            }
        }

        public Color backgroundColor {
            get {
                UIStyle style = FindActiveStyle((s) => s.paint.backgroundColor != UIStyle.UnsetColorValue);
                return style != null ? style.paint.backgroundColor : UIStyle.UnsetColorValue;
            }
            set {
                SetBackgroundColorForState(StyleStateType.InstanceNormal, value);
                view.MarkForRendering(element);
            }
        }

        public Texture2D backgroundImage {
            get {
                UIStyle style = FindActiveStyle((s) => s.paint.backgroundImage != null);
                return style != null ? style.paint.backgroundImage : null;
            }
            set {
                SetBackgroundImageForState(StyleStateType.InstanceNormal, value);
                view.MarkForRendering(element);
            }
        }

        public Color borderColor {
            get {
                UIStyle style = FindActiveStyle((s) => s.paint.borderColor != UIStyle.UnsetColorValue);
                return style != null ? style.paint.borderColor : UIStyle.UnsetColorValue;
            }
            set {
                SetBackgroundColorForState(StyleStateType.InstanceNormal, value);
                view.MarkForRendering(element);
            }
        }

        public UIStyleRect margin {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.margin != UIStyle.UnsetRectValue);
                return style != null ? style.contentBox.margin : UIStyle.UnsetRectValue;
            }
            set { SetMarginForState(StyleStateType.InstanceNormal, value); }
        }

        public UIStyleRect padding {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.padding != UIStyle.UnsetRectValue);
                return style != null ? style.contentBox.padding : UIStyle.UnsetRectValue;
            }
            set { SetPaddingForState(StyleStateType.InstanceNormal, value); }
        }

        public UIStyleRect border {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.border != UIStyle.UnsetRectValue);
                return style != null ? style.contentBox.border : UIStyle.UnsetRectValue;
            }
            set { SetBorderForState(StyleStateType.InstanceNormal, value); }
        }

        public float contentWidth {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.contentWidth != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.contentWidth : UIStyle.UnsetFloatValue;
            }
            set { SetContentWidthForState(StyleStateType.InstanceNormal, value); }
        }

        public float contentHeight {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.contentHeight != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.contentHeight : UIStyle.UnsetFloatValue;
            }
            set { SetContentHeightForState(StyleStateType.InstanceNormal, value); }
        }

        public Vector2 position {
            get {
                UIStyle style = FindActiveStyle((s) => s.transform.position != UIStyle.UnsetVector2Value);
                return style != null ? style.transform.position : UIStyle.UnsetVector2Value;
            }
            set { SetPositionForState(StyleStateType.InstanceNormal, value); }
        }

        private UIStyle GetStyleForState(StyleStateType state) {
            // only return instance styles
            state = state & ~(StyleStateType.Base);
            for (int i = 0; i < stylePairs.Length; i++) {
                StyleStateType checkFlag = stylePairs[i].checkFlag;
                UIStyle style = stylePairs[i].style;
                if ((checkFlag & state) != 0) {
                    return style;
                }
            }
            return null;
        }

        private UIStyle FindActiveStyle(Func<UIStyle, bool> callback) {
            for (int i = 0; i < stylePairs.Length; i++) {
                if ((stylePairs[i].checkFlag & currentStateType) != 0) {
                    if (callback(stylePairs[i].style)) {
                        return stylePairs[i].style;
                    }
                }
            }
            return null;
        }

        private UIStyle GetOrCreateStyleForState(StyleStateType state) {
            // only return instance styles
            state = state & ~(StyleStateType.Base);
            UIStyle retn = GetStyleForState(state);
            if (retn != null) return retn;
            UIStyle style = new UIStyle();
            SetInstanceStyle(style, state);
            return style;
        }

        public void SetBackgroundColorForState(StyleStateType state, Color color) {
            GetOrCreateStyleForState(state).paint.backgroundColor = color;
        }

        public Color GetBackgroundColorForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.paint.backgroundColor : UIStyle.UnsetColorValue;
        }

        public void SetBackgroundImageForState(StyleStateType state, Texture2D image) {
            GetOrCreateStyleForState(state).paint.backgroundImage = image;
        }

        public Texture2D GetBackgroundImageForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.paint.backgroundImage : null;
        }

        public void SetMarginForState(StyleStateType state, UIStyleRect margin) {
            GetOrCreateStyleForState(state).contentBox.margin = margin;
        }

        public UIStyleRect GetMarginForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.contentBox.margin : UIStyle.UnsetRectValue;
        }

        public void SetPaddingForState(StyleStateType state, UIStyleRect padding) {
            GetOrCreateStyleForState(state).contentBox.padding = padding;
        }

        public UIStyleRect GetPaddingForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.contentBox.padding : UIStyle.UnsetRectValue;
        }

        public float GetContentWidthForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.contentBox.contentWidth : UIStyle.UnsetFloatValue;
        }

        public void SetContentWidthForState(StyleStateType state, float value) {
            GetOrCreateStyleForState(state).contentBox.contentWidth = value;
        }

        public float GetContentHeightForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.contentBox.contentHeight : UIStyle.UnsetFloatValue;
        }

        public void SetContentHeightForState(StyleStateType state, float value) {
            GetOrCreateStyleForState(state).contentBox.contentHeight = value;
        }

        public Vector2 GetPositionForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.transform.position : UIStyle.UnsetVector2Value;
        }

        public void SetPositionForState(StyleStateType state, Vector2 value) {
            GetOrCreateStyleForState(state).transform.position = value;
        }

        public void SetBorderForState(StyleStateType state, UIStyleRect border) {
            GetOrCreateStyleForState(state).contentBox.border = border;
        }

        public UIStyleRect GetBorderForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.contentBox.border : UIStyle.UnsetRectValue;
        }

        public void SetBorderColorForState(StyleStateType state, Color color) {
            GetOrCreateStyleForState(state).paint.borderColor = color;
        }

        public Color GetBorderColorForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.paint.borderColor : UIStyle.UnsetColorValue;
        }

        public bool RequiresRendering() {
            return backgroundColor    != UIStyle.UnsetColorValue
                   && backgroundImage != null
                   && borderColor     != UIStyle.UnsetColorValue;
        }

        private void OnStyleChanged(UIStyle style) { }

        public struct UIStyleProxy {

            private readonly UIStyleSet styleSet;
            private readonly StyleStateType stateType;

            internal static UIStyle hack;

            public UIStyleProxy(UIStyleSet styleSet, StyleStateType stateType) {
                this.styleSet = styleSet;
                this.stateType = stateType;
            }

            public Color backgroundColor {
                get { return styleSet.GetBackgroundColorForState(stateType); }
                set { styleSet.SetBackgroundColorForState(stateType, value); }
            }

            public Texture2D backgroundImage {
                get { return styleSet.GetBackgroundImageForState(stateType); }
                set { styleSet.SetBackgroundImageForState(stateType, value); }
            }

            public Color borderColor {
                get { return styleSet.GetBorderColorForState(stateType); }
                set { styleSet.SetBorderColorForState(stateType, value); }
            }

            public UIStyleRect margin {
                get { return styleSet.GetMarginForState(stateType); }
                set { styleSet.SetMarginForState(stateType, value); }
            }

            public UIStyleRect padding {
                get { return styleSet.GetPaddingForState(stateType); }
                set { styleSet.SetPaddingForState(stateType, value); }
            }

            public float contentWidth {
                get { return styleSet.GetContentWidthForState(stateType); }
                set { styleSet.SetContentWidthForState(stateType, value); }
            }

            public float contentHeight {
                get { return styleSet.GetContentHeightForState(stateType); }
                set { styleSet.SetContentHeightForState(stateType, value); }
            }

            public Vector2 position {
                get { return styleSet.GetPositionForState(stateType); }
                set { styleSet.SetPositionForState(stateType, value); }
            }

            public UIStyleRect border {
                get { return styleSet.GetBorderForState(stateType); }
                set { styleSet.SetBorderForState(stateType, value); }
            }

            public static implicit operator UIStyleProxy(UIStyle style) {
                UIStyleProxy proxy = new UIStyleProxy(null, StyleStateType.Normal);
                hack = style;
                return proxy;
            }

        }

        private struct StyleFlagPair {

            public readonly UIStyle style;
            public readonly StyleStateType checkFlag;

            public StyleFlagPair(UIStyle style, StyleStateType checkFlag) {
                this.style = style;
                this.checkFlag = checkFlag;
            }

        }

    }

}