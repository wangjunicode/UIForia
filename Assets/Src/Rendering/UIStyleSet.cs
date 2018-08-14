using System;
using Src;
using Src.Layout;
using UnityEngine;

namespace Rendering {

    public class UIStyleSet {

        private StyleFlagPair[] stylePairs;
        private StyleStateType currentStateType;

        private UIView view;
        private UIElement element;

        public UIStyleSet(UIElement element, UIView view) {
            currentStateType = StyleStateType.Normal;
            this.element = element;
            this.view = view;
            stylePairs = new StyleFlagPair[1];
            stylePairs[0] = new StyleFlagPair(new UIStyle(), StyleStateType.InstanceNormal);
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

        // temp public
        public void SetInstanceStyle(UIStyle style, StyleStateType state) {
            state &= ~(StyleStateType.Base);
            state |= StyleStateType.Instance;
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

        public UILayout layout {
            get {
                UIStyle style = FindActiveStyle((s) => s.layoutType != LayoutType.Unset);
                LayoutType layoutType = style != null ? style.layoutType : UIStyle.Default.layoutType;

                switch (layoutType) {
                    default: return UILayout.Flow;
                }
            }
        }

        public UIMeasurement rectWidth {
            get {
                UIStyle style = FindActiveStyle((s) => s.rect.width != UIStyle.UnsetMeasurementValue);
                return style != null ? style.rect.width : UIStyle.Default.rect.width;
            }
        }

        public UIMeasurement rectHeight {
            get {
                UIStyle style = FindActiveStyle((s) => s.rect.height != UIStyle.UnsetMeasurementValue);
                return style != null ? style.rect.height : UIStyle.Default.rect.height;
            }
        }

        public LayoutDirection layoutDirection {
            get { return LayoutDirection.Column; }
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

        public ContentBoxRect margin {
            get { return new ContentBoxRect(marginTop, marginRight, marginBottom, marginLeft); }
//            set { SetMarginForState(StyleStateType.InstanceNormal, value); }
        }

        public float marginLeft {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.margin.left != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.margin.left : UIStyle.Default.contentBox.margin.left;
            }
        }
        
        public float marginRight {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.margin.right != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.margin.right : UIStyle.Default.contentBox.margin.right;
            }
        }
        
        public float marginTop {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.margin.top != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.margin.top : UIStyle.Default.contentBox.margin.top;
            }
        }
        
        public float marginBottom {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.margin.bottom != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.margin.bottom : UIStyle.Default.contentBox.margin.bottom;
            }
        }
        
        public ContentBoxRect padding {
            get { return new ContentBoxRect(paddingTop, paddingRight, paddingBottom, paddingLeft); }
            //set { SetPaddingForState(StyleStateType.InstanceNormal, value); }
        }

        public float paddingLeft {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.padding.left != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.padding.left : UIStyle.Default.contentBox.padding.left;
            }
        }
        
        public float paddingRight {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.padding.right != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.padding.right : UIStyle.Default.contentBox.padding.right;
            }
        }
        
        public float paddingTop {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.padding.top != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.padding.top : UIStyle.Default.contentBox.padding.top;
            }
        }
        
        public float paddingBottom {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.padding.bottom != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.padding.bottom : UIStyle.Default.contentBox.padding.bottom;
            }
        }
        
        public ContentBoxRect border {
            get {return new ContentBoxRect(borderTop, borderRight, borderBottom, borderLeft); }
//            set { SetBorderForState(StyleStateType.InstanceNormal, value); }
        }

        public float borderLeft {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.border.left != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.border.left : UIStyle.Default.contentBox.border.left;
            }
        }
        
        public float borderRight {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.border.right != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.border.right : UIStyle.Default.contentBox.border.right;
            }
        }
        
        public float borderTop {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.border.top != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.border.top : UIStyle.Default.contentBox.border.top;
            }
        }
        
        public float borderBottom {
            get {
                UIStyle style = FindActiveStyle((s) => s.contentBox.border.bottom != UIStyle.UnsetFloatValue);
                return style != null ? style.contentBox.border.bottom : UIStyle.Default.contentBox.border.bottom;
            }
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

        public void SetMarginForState(StyleStateType state, ContentBoxRect margin) {
            GetOrCreateStyleForState(state).contentBox.margin = margin;
        }

        public ContentBoxRect GetMarginForState(StyleStateType state) {
            UIStyle style = GetStyleForState(state);
            return (style != null) ? style.contentBox.margin : UIStyle.UnsetRectValue;
        }

        public void SetPaddingForState(StyleStateType state, ContentBoxRect padding) {
            GetOrCreateStyleForState(state).contentBox.padding = padding;
        }

        public ContentBoxRect GetPaddingForState(StyleStateType state) {
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

        public void SetBorderForState(StyleStateType state, ContentBoxRect border) {
            GetOrCreateStyleForState(state).contentBox.border = border;
        }

        public ContentBoxRect GetBorderForState(StyleStateType state) {
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

            public ContentBoxRect margin {
                get { return styleSet.GetMarginForState(stateType); }
                set { styleSet.SetMarginForState(stateType, value); }
            }

            public ContentBoxRect padding {
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

            public ContentBoxRect border {
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