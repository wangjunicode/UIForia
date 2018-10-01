using Src;
using Src.Extensions;
using Src.Util;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        public Color GetTextColor(StyleState state) {
            return GetColorValue(StylePropertyId.TextColor, state);
        }

        public void SetTextColor(Color color, StyleState state) {
            SetColorProperty(StylePropertyId.TextColor, color, state);
        }

        public AssetPointer<Font> GetFont(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TextFontAsset, state);
            return property.IsDefined
                ? new AssetPointer<Font>(AssetType.Texture, property.valuePart1)
                : new AssetPointer<Font>(AssetType.Texture, IntUtil.UnsetValue);
        }

        public void SetFont(AssetPointer<Font> newFont, StyleState state) {
            SetAssetPointerProperty(StylePropertyId.TextFontAsset, newFont, state);
        }

        public int GetFontSize(StyleState state) {
            return GetIntValue(StylePropertyId.TextFontSize, state);
        }

        public void SetFontSize(int newFontSize, StyleState state) {
            SetIntProperty(StylePropertyId.TextFontSize, newFontSize, state);
        }

        public TextUtil.FontStyle GetFontStyle(StyleState state) {
            return (TextUtil.FontStyle) GetEnumValue(StylePropertyId.TextFontStyle, state);
        }

        public void SetFontStyle(TextUtil.FontStyle newFontStyle, StyleState state) {
            SetEnumProperty(StylePropertyId.TextFontStyle, (int) newFontStyle, state);
        }

        public TextUtil.TextAnchor GetTextAnchor(StyleState state) {
            return (TextUtil.TextAnchor) GetEnumValue(StylePropertyId.TextAnchor, state);
        }

        public void SetTextAnchor(TextUtil.TextAnchor newTextAnchor, StyleState state) {
            SetEnumProperty(StylePropertyId.TextFontStyle, (int) newTextAnchor, state);
        }

    }

}