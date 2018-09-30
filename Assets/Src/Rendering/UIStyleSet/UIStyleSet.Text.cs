using Src;
using Src.Extensions;
using Src.Util;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {


        public Color GetTextColor(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TextColor, state);
            return property.IsDefined
                ? (Color) new StyleColor(property.valuePart0)
                : ColorUtil.UnsetValue;
        }
        
        public void SetTextColor(Color color, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TextColor = color;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TextColor)) {
                computedStyle.TextColor = color;
            }
        }

        public AssetPointer<Font> GetFont(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TextFontAsset, state);
            return property.IsDefined
                ? new AssetPointer<Font>(AssetType.Texture, property.valuePart1)
                : new AssetPointer<Font>(AssetType.Texture, IntUtil.UnsetValue);
        }

        public void SetFont(AssetPointer<Font> newFont, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FontAsset = newFont;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TextFontAsset)) {
                computedStyle.FontAsset = newFont;
            }
        }

        public int GetFontSize(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TextFontSize, state);
            return property.IsDefined
                ? property.valuePart0
                : IntUtil.UnsetValue;
        }

        public void SetFontSize(int newFontSize, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FontSize = newFontSize;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TextFontSize)) {
                computedStyle.FontSize = newFontSize;
            }
        }

        public TextUtil.FontStyle GetFontStyle(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TextFontStyle, state);
            return property.IsDefined
                ? (TextUtil.FontStyle)property.valuePart0
                : TextUtil.FontStyle.Unset;
        }

        public void SetFontStyle(TextUtil.FontStyle newFontStyle, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FontStyle = newFontStyle;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TextFontStyle)) {
                computedStyle.FontStyle = newFontStyle;
            }
        }

        public TextUtil.TextAnchor GetTextAnchor(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TextAnchor, state);
            return property.IsDefined
                ? (TextUtil.TextAnchor)property.valuePart0
                : TextUtil.TextAnchor.Unset;
        }

        public void SetTextAnchor(TextUtil.TextAnchor newTextAnchor, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TextAnchor = newTextAnchor;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TextAnchor)) {
                computedStyle.TextAnchor = newTextAnchor;
            }
        }

//        public WhitespaceMode GetWhitespace(StyleState state) {
//            return GetStyle(state).textStyle.whiteSpace;
//        }
//
//        public void SetWhitespace(WhitespaceMode newWhitespace, StyleState state) {
//            UIStyle style = GetOrCreateInstanceStyle(state);
//            style.textStyle.whiteSpace = newWhitespace;
//            computedStyle.textStyle.whiteSpace = GetComputedWhitespaceMode();
//            if (whiteSpace == newWhitespace) {
//                styleSystem.SetTextStyle(element, textStyle);
//            }
//        }
//
//        public HorizontalWrapMode GetTextWrapHorizontal(StyleState state) {
//            return GetStyle(state).textStyle.horizontalOverflow;
//        }
//
//        public void SetTextWrapHorizontal(HorizontalWrapMode newWrapMode, StyleState state) {
//            UIStyle style = GetOrCreateInstanceStyle(state);
//            style.textStyle.horizontalOverflow = newWrapMode;
//            if (textWrap == newWrapMode) {
//                styleSystem.SetTextStyle(element, textStyle);
//            }
//        }
//
//        public VerticalWrapMode GetTextWrapVertical(StyleState state) {
//            return GetStyle(state).textStyle.verticalOverflow;
//        }
//
//        public void SetTextWrapVertical(VerticalWrapMode newWrapMode, StyleState state) {
//            UIStyle style = GetOrCreateInstanceStyle(state);
//            style.textStyle.verticalOverflow = newWrapMode;
//            if (textOverflow == newWrapMode) {
//                styleSystem.SetTextStyle(element, textStyle);
//            }
//        }
//
//        private TextStyle GetComputedText() {
//            return new TextStyle(
//                GetComputedFontColor(),
//                GetComputedFont(),
//                GetComputedFontSize(),
//                GetComputedFontStyle(),
//                GetComputedTextAnchor(),
//                GetComputedWhitespaceMode(),
//                GetComputedHorizontalTextWrap(),
//                GetComputedVerticalTextOverflow()
//            );
//        }
//
//        private Color GetComputedFontColor() {
//            return FindActiveStyle(s => s.textStyle.color.IsDefined()).textStyle.color;
//        }
//
//        private Font GetComputedFont() {
//            return FindActiveStyle(s => s.textStyle.font != null).textStyle.font;
//        }
//
//        private int GetComputedFontSize() {
//            return FindActiveStyle((s) => s.textStyle.fontSize != IntUtil.UnsetValue).textStyle.fontSize;
//        }
//
//        private FontStyle GetComputedFontStyle() {
//            return FindActiveStyle((s) => (int) s.textStyle.fontStyle != -1).textStyle.fontStyle;
//        }
//
//        private TextAnchor GetComputedTextAnchor() {
//            return FindActiveStyle((s) => (int) s.textStyle.alignment != -1).textStyle.alignment;
//        }
//
//        private HorizontalWrapMode GetComputedHorizontalTextWrap() {
//            return FindActiveStyle((s) => (int) s.textStyle.horizontalOverflow != -1).textStyle.horizontalOverflow;
//        }
//
//        private VerticalWrapMode GetComputedVerticalTextOverflow() {
//            return FindActiveStyle((s) => (int) s.textStyle.verticalOverflow != -1).textStyle.verticalOverflow;
//        }
//
//        private WhitespaceMode GetComputedWhitespaceMode() {
//            return FindActiveStyle((s) => s.textStyle.whiteSpace != WhitespaceMode.Unset).textStyle.whiteSpace;
//        }

    }

}