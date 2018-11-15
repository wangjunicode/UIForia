using TMPro;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Rendering {

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

        public bool Defines(StylePropertyId propertyId) {
            return false;//return m_DefinedStyles.Contains(propertyId);
        }
        
        public void SetFontSize(int newFontSize, StyleState state) {
            if (newFontSize == IntUtil.UnsetValue) {
                UIElement ptr = element.parent;

                while (ptr != null) {
                    if (ptr.style.Defines(StylePropertyId.TextFontSize)) {
                        break;
                    }

                    ptr = ptr.parent;
                }

                if (ptr != null) {
                   // ptr.style.UpdateInheritedProperty(StylePropertyId.TextFontSize);
                }
            }
            else {
//                SetIntProperty(StylePropertyId.TextFontSize, newFontSize, state);
            }
        }

        public Text.FontStyle GetFontStyle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontStyle, state).AsFontStyle;
        }

        public void SetFontStyle(Text.FontStyle newFontStyle, StyleState state) {
            SetEnumProperty(StylePropertyId.TextFontStyle, (int) newFontStyle, state);
        }

        public Text.TextAlignment GetTextAnchor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextAlignment, state).AsTextAlignment;
        }

        public void SetTextAnchor(Text.TextAlignment newTextAlignment, StyleState state) {
            SetEnumProperty(StylePropertyId.TextFontStyle, (int) newTextAlignment, state);
        }

    }

}