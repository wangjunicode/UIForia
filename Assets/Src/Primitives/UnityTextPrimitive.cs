using Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public class UnityTextPrimitive : TextPrimitive {

        private readonly Text textComponent;

        public UnityTextPrimitive(Text textComponent) {
            this.textComponent = textComponent;
        }

        public override string Text {
            get { return textComponent.text; }
            set { textComponent.text = value; }
        }

        public override void ApplyFontSettings(TextStyle fontSettings) {
            textComponent.font = fontSettings.font;
            textComponent.fontStyle = (FontStyle) fontSettings.fontStyle;
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.fontSize = fontSettings.fontSize;
            textComponent.color = (Color) fontSettings.color;
            textComponent.rectTransform.SetSize(textComponent.preferredWidth, textComponent.preferredHeight);
            textComponent.verticalOverflow = fontSettings.verticalOverflow;
            textComponent.horizontalOverflow = fontSettings.horizontalOverflow;

        }

        private float CalculateLineHeight(Text text) {
            var extents = text.cachedTextGenerator.rectExtents.size * 0.5f;
            var lineHeight = text.cachedTextGeneratorForLayout.GetPreferredHeight("A", text.GetGenerationSettings(extents));

            return lineHeight;
        }

    }

    public static class RectTransformExtensions {

        public static void SetSize(this RectTransform transform, float width, float height) {
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

    }

}