using TMPro;
using UnityEngine;

namespace Rendering {

    public class GOTextSizeCalculator : ITextSizeCalculator {

        private readonly TextMeshProUGUI ruler;

        public GOTextSizeCalculator() {
            GameObject gameObject = new GameObject("Ruler");
            ruler = gameObject.AddComponent<TextMeshProUGUI>();
            ruler.richText = false;
            ruler.raycastTarget = false;
        }

        public float CalcTextWidth(string text, UIStyleSet style) {
            ruler.fontSize = style.fontSize;
            ruler.text = text + "\u200B";
            return Mathf.CeilToInt(ruler.GetPreferredValues().x + 1);
        }

        public float CalcTextHeight(string text, UIStyleSet style, float width) {
            ruler.fontSize = style.fontSize;
            ruler.text = text + "\u200B";
            return Mathf.CeilToInt(ruler.GetPreferredValues(width + 1, 0).y);
        }

    }

}