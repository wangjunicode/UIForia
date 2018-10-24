using JetBrains.Annotations;
using Src;
using Src.Extensions;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public Color GetBackgroundColor(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BackgroundColor, state);
            return property.IsDefined
                ? (Color) new StyleColor(property.valuePart0)
                : ColorUtil.UnsetValue;
        }
        
        [PublicAPI]
        public Color GetBorderColor(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderColor, state);
            return property.IsDefined
                ? (Color) new StyleColor(property.valuePart0)
                : ColorUtil.UnsetValue;
        }

        [PublicAPI]
        public Texture2D GetBackgroundImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColor, state).AsTexture;
        }
        
        [PublicAPI]
        public void SetBackgroundColor(Color color, StyleState state) {
            SetColorProperty(StylePropertyId.BackgroundColor, color, state);
        }

        [PublicAPI]
        public void SetBackgroundImage(Texture2D image, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BackgroundImage = image;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BackgroundImage)) {
                computedStyle.BackgroundImage = image;
            }
        }

        [PublicAPI]
        public void SetBorderColor(Color color, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderColor = color;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderColor)) {
                computedStyle.BorderColor = color;
            }
        }



    }

}