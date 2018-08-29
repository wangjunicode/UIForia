using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        public TextStyle textStyle {
            // todo -- this needs to do property look ups WITHOUT checking the default style
            get { return new TextStyle(textColor, font, fontSize, fontStyle, textAnchor, whiteSpace, textWrap, textOverflow); }
            set { SetTextStyle(value, StyleState.Normal); }
        }

        public Color textColor {
            get { return FindActiveStyle((s) => ColorUtil.IsDefined(s.text.color)).text.color; }
            set { SetTextColor(value, StyleState.Normal); }
        }

        public Font font {
            get { return FindActiveStyle((s) => s.text.font != null).text.font; }
            set { SetFont(value, StyleState.Normal); }
        }

        public int fontSize {
            get { return FindActiveStyle((s) => s.text.fontSize != IntUtil.UnsetValue).text.fontSize; }
            set { SetFontSize(value, StyleState.Normal); }
        }

        public FontStyle fontStyle {
            get { return FindActiveStyle((s) => (int) s.text.fontStyle != -1).text.fontStyle; }
            set { SetFontStyle(value, StyleState.Normal); }
        }

        public TextAnchor textAnchor {
            get { return FindActiveStyle((s) => (int) s.text.alignment != -1).text.alignment; }
            set { SetTextAnchor(value, StyleState.Normal); }
        }

        public HorizontalWrapMode textWrap {
            get { return FindActiveStyle((s) => (int) s.text.horizontalOverflow != -1).text.horizontalOverflow; }
            set { SetTextWrapHorizontal(value, StyleState.Normal); }
        }

        public VerticalWrapMode textOverflow {
            get { return FindActiveStyle((s) => (int) s.text.verticalOverflow != -1).text.verticalOverflow; }
            set { SetTextWrapVertical(value, StyleState.Normal); }
        }

        public WhitespaceMode whiteSpace {
            get { return FindActiveStyle((s) => s.text.whiteSpace != WhitespaceMode.Unset).text.whiteSpace; }
            set { SetWhitespace(value, StyleState.Normal); }
        }

        public TextStyle GetTextStyle(StyleState state) {
            return GetStyle(state).text;
        }

        public void SetTextStyle(TextStyle newStyle, StyleState state) {
            GetOrCreateStyle(state).text = newStyle;
            changeHandler.SetText(elementId, textStyle);
        }

        public Color GetTextColor(StyleState state) {
            return GetStyle(state).text.color;
        }

        public void SetTextColor(Color color, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.text.color = color;
            if (textColor == color) {
                changeHandler.SetText(elementId, textStyle);
            }
        }

        public Font GetFont(StyleState state) {
            return GetStyle(state).text.font;
        }

        public void SetFont(Font newFont, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.text.font = newFont;
            if (font == newFont) {
                changeHandler.SetText(elementId, textStyle);
            }
        }

        public int GetFontSize(StyleState state) {
            return GetStyle(state).text.fontSize;
        }

        public void SetFontSize(int newFontSize, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.text.fontSize = newFontSize;
            if (fontSize == newFontSize) {
                changeHandler.SetText(elementId, textStyle);
            }
        }

        public FontStyle GetFontStyle(StyleState state) {
            return GetStyle(state).text.fontStyle;
        }

        public void SetFontStyle(FontStyle newFontStyle, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.text.fontStyle = newFontStyle;
            if (fontStyle == newFontStyle) {
                changeHandler.SetText(elementId, textStyle);
            }
        }

        public TextAnchor GetTextAnchor(StyleState state) {
            return GetStyle(state).text.alignment;
        }

        public void SetTextAnchor(TextAnchor newTextAnchor, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.text.alignment = newTextAnchor;
            if (textAnchor == newTextAnchor) {
                changeHandler.SetText(elementId, textStyle);
            }
        }

        public WhitespaceMode GetWhitespace(StyleState state) {
            return GetStyle(state).text.whiteSpace;
        }

        public void SetWhitespace(WhitespaceMode newWhitespace, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.text.whiteSpace = newWhitespace;
            if (whiteSpace == newWhitespace) {
                changeHandler.SetText(elementId, textStyle);
            }
        }
        
        public HorizontalWrapMode GetTextWrapHorizontal(StyleState state) {
            return GetStyle(state).text.horizontalOverflow;
        }

        public void SetTextWrapHorizontal(HorizontalWrapMode newWrapMode, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.text.horizontalOverflow = newWrapMode;
            if (textWrap == newWrapMode) {
                changeHandler.SetText(elementId, textStyle);
            }
        }

        public VerticalWrapMode GetTextWrapVertical(StyleState state) {
            return GetStyle(state).text.verticalOverflow;
        }

        public void SetTextWrapVertical(VerticalWrapMode newWrapMode, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.text.verticalOverflow = newWrapMode;
            if (textOverflow == newWrapMode) {
                changeHandler.SetText(elementId, textStyle);
            }
        }

    }

}