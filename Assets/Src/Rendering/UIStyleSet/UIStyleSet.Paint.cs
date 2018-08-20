using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        #region Paint

        public Color backgroundColor {
            get { return FindActiveStyle((s) => s.paint.backgroundColor != UIStyle.UnsetColorValue).paint.backgroundColor; }
            set { SetBackgroundColor(value); }
        }

        public Texture2D backgroundImage {
            get { return FindActiveStyle((s) => s.paint.backgroundImage != null).paint.backgroundImage; }
            set { SetBackgroundImage(value); }
        }

        public Color borderColor {
            get { return FindActiveStyle((s) => s.paint.borderColor != UIStyle.UnsetColorValue).paint.borderColor; }
            set { SetBackgroundColor(value); }
        }

        public Color GetBackgroundColor(StyleState state) {
            UIStyle style = GetStyle(state);
            return (style != null) ? style.paint.backgroundColor : UIStyle.UnsetColorValue;
        }
        
        public void SetBackgroundColor(Color color, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).paint.backgroundColor = color;
            if (backgroundColor == color) {
                view.renderSystem.OnElementStyleChanged(element);
            }
        }

        public void SetBackgroundImage(Texture2D image, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).paint.backgroundImage = image;
            if (backgroundImage == image) {
                view.renderSystem.OnElementStyleChanged(element);
            }
        }

        public Texture2D GetBackgroundImage(StyleState state) {
            return GetStyle(state).paint.backgroundImage;
        }

        public void SetBorderColor(Color color, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).paint.borderColor = color;
            if (backgroundColor == color) {
                view.renderSystem.OnElementStyleChanged(element);
            }
        }

        public Color GetBorderColor(StyleState state) {
            return GetStyle(state).paint.borderColor;
        }

        #endregion

    }

}