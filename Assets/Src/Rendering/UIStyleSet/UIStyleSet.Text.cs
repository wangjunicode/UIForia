using Src;
using Src.Util;
using TMPro;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        public Color GetTextColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextColor, state).AsColor;
        }

        public void SetTextColor(Color color, StyleState state) {
            SetColorProperty(StylePropertyId.TextColor, color, state);
        }

        public TMP_FontAsset GetFont(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont;
        }

        public void SetFont(TMP_FontAsset newFont, StyleState state) {
            SetObjectProperty(StylePropertyId.TextFontAsset, newFont, state);
        }

        public int GetFontSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsInt;
        }

        public void SetFontSize(int newFontSize, StyleState state) {
            SetIntProperty(StylePropertyId.TextFontSize, newFontSize, state);
        }

        public TextUtil.FontStyle GetFontStyle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontStyle, state).AsFontStyle;
        }

        public void SetFontStyle(TextUtil.FontStyle newFontStyle, StyleState state) {
            SetEnumProperty(StylePropertyId.TextFontStyle, (int) newFontStyle, state);
        }

        public TextUtil.TextAlignment GetTextAnchor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextAlignment, state).AsTextAlignment;
        }

        public void SetTextAnchor(TextUtil.TextAlignment newTextAlignment, StyleState state) {
            SetEnumProperty(StylePropertyId.TextFontStyle, (int) newTextAlignment, state);
        }

    }

}