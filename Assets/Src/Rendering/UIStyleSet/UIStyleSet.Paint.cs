using JetBrains.Annotations;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public Paint paint {
            get { return new Paint(backgroundColor, borderColor, backgroundImage); }
            set { SetPaint(value); }
        }

        [PublicAPI]
        public Color backgroundColor {
            get {
                return FindActiveStyle((s) => s.paint.backgroundColor != UIStyle.UnsetColorValue).paint.backgroundColor;
            }
            set { SetBackgroundColor(value); }
        }

        [PublicAPI]
        public Texture2D backgroundImage {
            get { return FindActiveStyle((s) => s.paint.backgroundImage != null).paint.backgroundImage; }
            set { SetBackgroundImage(value); }
        }

        [PublicAPI]
        public Color borderColor {
            get { return FindActiveStyle((s) => s.paint.borderColor != UIStyle.UnsetColorValue).paint.borderColor; }
            set { SetBackgroundColor(value); }
        }

        [PublicAPI]
        public void SetPaint(Paint newPaint, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).paint = newPaint;
            if (paint == newPaint) { }
        }

        [PublicAPI]
        public Color GetBackgroundColor(StyleState state) {
            return GetStyle(state).paint.backgroundColor;
        }

        [PublicAPI]
        public void SetBackgroundColor(Color color, StyleState state = StyleState.Normal) {
            UIStyle target = GetOrCreateStyle(state);
            target.paint = new Paint(color, target.paint.borderColor, target.paint.backgroundImage);
            if (backgroundColor == color) {
                changeHandler.SetPaint(elementId, paint);
            }
        }

        [PublicAPI]
        public void SetBackgroundImage(Texture2D image, StyleState state = StyleState.Normal) {
            UIStyle target = GetOrCreateStyle(state);
            target.paint = new Paint(
                target.paint.backgroundColor,
                target.paint.borderColor,
                image
            );

            if (backgroundImage == image) {
                changeHandler.SetPaint(elementId, paint);
            }
        }

        [PublicAPI]
        public Texture2D GetBackgroundImage(StyleState state) {
            return GetStyle(state).paint.backgroundImage;
        }

        [PublicAPI]
        public void SetBorderColor(Color color, StyleState state = StyleState.Normal) {
            UIStyle target = GetOrCreateStyle(state);
            target.paint = new Paint(
                target.paint.backgroundColor,
                color,
                target.paint.backgroundImage
            );
            if (borderColor == color) {
                changeHandler.SetPaint(elementId, paint);
            }
        }

        [PublicAPI]
        public Color GetBorderColor(StyleState state) {
            return GetStyle(state).paint.borderColor;
        }

    }

}