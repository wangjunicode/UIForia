using TMPro;
using UnityEngine;

namespace Rendering {
    
    public struct TextSizeResult {

        public readonly float preferredWidth;
        public readonly float minWidth;

        public TextSizeResult(float minWidth, float preferredWidth) {
            this.minWidth = minWidth;
            this.preferredWidth = preferredWidth;
        }
    }
    
    public class GOTextSizeCalculator : ITextSizeCalculator {

        private readonly TextMeshProUGUI ruler;

        public GOTextSizeCalculator() {
            GameObject gameObject = new GameObject("Ruler");
            ruler = gameObject.AddComponent<TextMeshProUGUI>();
            ruler.richText = false;
            ruler.raycastTarget = false;
        }

        public TextSizeResult CalcTextPreferredAndMinWidth(UIStyleSet style) {
            ruler.fontSize = style.fontSize;
            ruler.text = style.textContent;

            float preferredWidth = Mathf.CeilToInt(ruler.GetPreferredValues().x);

            TMP_WordInfo[] wordInfos = ruler.textInfo.wordInfo;
            TMP_CharacterInfo[] characterInfos = ruler.textInfo.characterInfo;
            
            float maxWordSize = 0;
            for (int i = 0; i < wordInfos.Length; i++) {
                float start = characterInfos[wordInfos[i].firstCharacterIndex].vertex_TL.position.x;
                float end = characterInfos[wordInfos[i].lastCharacterIndex].vertex_TR.position.x;
                float size = end - start;
                if (size > maxWordSize) {
                    maxWordSize = size;
                }
            }
            
            return new TextSizeResult(maxWordSize, preferredWidth);
        }
        
        public float CalcTextWidth(string text, UIStyleSet style) {
            ruler.fontSize = style.fontSize;
            ruler.text = text;

            float preferredWidth = Mathf.CeilToInt(ruler.GetPreferredValues().x);

            TMP_WordInfo[] wordInfos = ruler.textInfo.wordInfo;
            TMP_CharacterInfo[] characterInfos = ruler.textInfo.characterInfo;
            
            float maxWordSize = 0;
            for (int i = 0; i < wordInfos.Length; i++) {
                float start = characterInfos[wordInfos[i].firstCharacterIndex].vertex_TL.position.x;
                float end = characterInfos[wordInfos[i].lastCharacterIndex].vertex_TR.position.x;
                float size = end - start;
                if (size > maxWordSize) {
                    maxWordSize = size;
                }
            }
            
            float minWidth = maxWordSize;
            
            return preferredWidth;
        }

        public float CalcTextHeight(string text, UIStyleSet style, float width) {
            ruler.fontSize = style.fontSize;
            ruler.text = text;
            return Mathf.CeilToInt(ruler.GetPreferredValues(width, 0).y);
        }

    }

}