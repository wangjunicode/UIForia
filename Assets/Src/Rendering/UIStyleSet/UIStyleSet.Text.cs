using Src.Extensions;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        public TextStyle textStyle {
            get { return computedStyle.textStyle; }
            set { SetTextStyle(value, StyleState.Normal); }
        }

        public Color textColor {
            get { return computedStyle.textStyle.color; }
            set { SetTextColor(value, StyleState.Normal); }
        }

        public Font font {
            get { return computedStyle.textStyle.font; }
            set { SetFont(value, StyleState.Normal); }
        }

        public int fontSize {
            get { return computedStyle.textStyle.fontSize; }
            set { SetFontSize(value, StyleState.Normal); }
        }

        public FontStyle fontStyle {
            get { return computedStyle.textStyle.fontStyle; }
            set { SetFontStyle(value, StyleState.Normal); }
        }

        public TextAnchor textAnchor {
            get { return computedStyle.textStyle.alignment; }
            set { SetTextAnchor(value, StyleState.Normal); }
        }

        public HorizontalWrapMode textWrap {
            get { return computedStyle.textStyle.horizontalOverflow; }
            set { SetTextWrapHorizontal(value, StyleState.Normal); }
        }

        public VerticalWrapMode textOverflow {
            get { return computedStyle.textStyle.verticalOverflow; }
            set { SetTextWrapVertical(value, StyleState.Normal); }
        }

        public WhitespaceMode whiteSpace {
            get { return computedStyle.textStyle.whiteSpace; }
            set { SetWhitespace(value, StyleState.Normal); }
        }

        public TextStyle GetTextStyle(StyleState state) {
            return GetStyle(state).textStyle;
        }

        public void SetTextStyle(TextStyle newStyle, StyleState state) {
            GetOrCreateStyle(state).textStyle = newStyle;
            changeHandler.SetTextStyle(elementId, textStyle);
        }

        public Color GetTextColor(StyleState state) {
            return GetStyle(state).textStyle.color;
        }

        public void SetTextColor(Color color, StyleState state) {
//            UIStyle style = GetOrCreateStyle(state);
//            style.textStyle.color = color;
//            if (textColor == color) {
//                changeHandler.SetTextStyle(elementId, textStyle);
//            }
            UIStyle targetStyle = GetOrCreateStyle(state);
            targetStyle.textStyle.color = color;

            // no changes needed if target state is not active
            if ((state & currentState) == 0) {
                return;
            }

            UIStyle activeStyle = FindActiveStyleWithoutDefault((s) => s.textStyle.color.IsDefined());

            if (!color.IsDefined()) {
                ownTextStyle.color = activeStyle?.textStyle.color ?? ColorUtil.UnsetValue;
                styleSystem.SetFontColor(elementId, color);
                changeHandler.SetTextStyle(elementId, computedStyle.textStyle);
                return;
            }

            if (targetStyle != activeStyle) {
                return;
            }
            
            if (ownTextStyle.color == color) {
                return;
            }
            
            ownTextStyle.color = color;
            styleSystem.SetFontColor(elementId, color);
            changeHandler.SetTextStyle(elementId, computedStyle.textStyle);
        }

        public Font GetFont(StyleState state) {
            return GetStyle(state).textStyle.font;
        }

        public void SetFont(Font newFont, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.textStyle.font = newFont;
            if (font == newFont) {
                changeHandler.SetTextStyle(elementId, textStyle);
            }
        }

        public int GetFontSize(StyleState state) {
            return GetStyle(state).textStyle.fontSize;
        }

        public void SetFontSize(int newFontSize, StyleState state) {
            // if new value is unset -> 
            //    if we have a different applied font style (ie hover, base style, whatever)
            //        set ownTextStyle.fontSize to that and update font tree
            // if new value is not unset
            //    if the input state is active
            //        get the current style that the topmost font size comes from
            //        if that style is the state target (state is equal and type is instance)
            //            setOwnTextStyle.fontSize
            //            update font tree
            //        else
            //            return
            
            // set state's fontSize to new value
            UIStyle targetStyle = GetOrCreateStyle(state);
            targetStyle.textStyle.fontSize = newFontSize;

            // no changes needed if target state is not active
            if ((state & currentState) == 0) {
                return;
            }

            UIStyle activeStyle = FindActiveStyleWithoutDefault((s) => IntUtil.IsDefined(s.textStyle.fontSize));

            if (!IntUtil.IsDefined(newFontSize)) {
                ownTextStyle.fontSize = activeStyle?.textStyle.fontSize ?? IntUtil.UnsetValue;
                styleSystem.SetFontSize(elementId, newFontSize);
                changeHandler.SetTextStyle(elementId, computedStyle.textStyle);
                return;
            }

            if (targetStyle != activeStyle) {
                return;
            }
            
            if (ownTextStyle.fontSize == newFontSize) {
                return;
            }
            
            ownTextStyle.fontSize = newFontSize;
            styleSystem.SetFontSize(elementId, newFontSize);
            changeHandler.SetTextStyle(elementId, computedStyle.textStyle);
            
        }

        public FontStyle GetFontStyle(StyleState state) {
            return GetStyle(state).textStyle.fontStyle;
        }

        public void SetFontStyle(FontStyle newFontStyle, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.textStyle.fontStyle = newFontStyle;
            FontStyle computed = GetComputedFontStyle();
            FontStyle current = computedStyle.textStyle.fontStyle;
            if (current != newFontStyle && computed == newFontStyle) {
                changeHandler.SetTextStyle(elementId, textStyle);
            }
        }

        public TextAnchor GetTextAnchor(StyleState state) {
            return GetStyle(state).textStyle.alignment;
        }

        public void SetTextAnchor(TextAnchor newTextAnchor, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.textStyle.alignment = newTextAnchor;
            computedStyle.textStyle.alignment = GetComputedTextAnchor();
            if (textAnchor == newTextAnchor) {
                changeHandler.SetTextStyle(elementId, textStyle);
            }
        }

        public WhitespaceMode GetWhitespace(StyleState state) {
            return GetStyle(state).textStyle.whiteSpace;
        }

        public void SetWhitespace(WhitespaceMode newWhitespace, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.textStyle.whiteSpace = newWhitespace;
            computedStyle.textStyle.whiteSpace = GetComputedWhitespaceMode();
            if (whiteSpace == newWhitespace) {
                changeHandler.SetTextStyle(elementId, textStyle);
            }
        }

        public HorizontalWrapMode GetTextWrapHorizontal(StyleState state) {
            return GetStyle(state).textStyle.horizontalOverflow;
        }

        public void SetTextWrapHorizontal(HorizontalWrapMode newWrapMode, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.textStyle.horizontalOverflow = newWrapMode;
            if (textWrap == newWrapMode) {
                changeHandler.SetTextStyle(elementId, textStyle);
            }
        }

        public VerticalWrapMode GetTextWrapVertical(StyleState state) {
            return GetStyle(state).textStyle.verticalOverflow;
        }

        public void SetTextWrapVertical(VerticalWrapMode newWrapMode, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.textStyle.verticalOverflow = newWrapMode;
            if (textOverflow == newWrapMode) {
                changeHandler.SetTextStyle(elementId, textStyle);
            }
        }

        private TextStyle GetComputedText() {
            return new TextStyle(
                GetComputedFontColor(),
                GetComputedFont(),
                GetComputedFontSize(),
                GetComputedFontStyle(),
                GetComputedTextAnchor(),
                GetComputedWhitespaceMode(),
                GetComputedHorizontalTextWrap(),
                GetComputedVerticalTextOverflow()
            );
        }

        private Color GetComputedFontColor() {
            return FindActiveStyle(s => s.textStyle.color.IsDefined()).textStyle.color;
        }

        private Font GetComputedFont() {
            return FindActiveStyle(s => s.textStyle.font != null).textStyle.font;
        }

        private int GetComputedFontSize() {
            return FindActiveStyle((s) => s.textStyle.fontSize != IntUtil.UnsetValue).textStyle.fontSize;
        }

        private FontStyle GetComputedFontStyle() {
            return FindActiveStyle((s) => (int) s.textStyle.fontStyle != -1).textStyle.fontStyle;
        }

        private TextAnchor GetComputedTextAnchor() {
            return FindActiveStyle((s) => (int) s.textStyle.alignment != -1).textStyle.alignment;
        }

        private HorizontalWrapMode GetComputedHorizontalTextWrap() {
            return FindActiveStyle((s) => (int) s.textStyle.horizontalOverflow != -1).textStyle.horizontalOverflow;
        }

        private VerticalWrapMode GetComputedVerticalTextOverflow() {
            return FindActiveStyle((s) => (int) s.textStyle.verticalOverflow != -1).textStyle.verticalOverflow;
        }

        private WhitespaceMode GetComputedWhitespaceMode() {
            return FindActiveStyle((s) => s.textStyle.whiteSpace != WhitespaceMode.Unset).textStyle.whiteSpace;
        }

    }

}