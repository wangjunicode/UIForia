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
            ruler.text = text;
            ruler.fontSize = style.fontSize;
            //ruler.ForceMeshUpdate();
            return ruler.GetPreferredValues().x + 1;
        }

        public float CalcTextHeight(string text, UIStyleSet style, float width) {
            ruler.text = text;
            ruler.fontSize = style.fontSize;
           // ruler.ForceMeshUpdate();
            return ruler.GetPreferredValues(width, 0).y;
        }
        
    }

}