using UnityEngine;

namespace Rendering {

    public class GOTextSizeCalculator : ITextSizeCalculator {
        
        // funny enough the editor and the game object UI text both use the same
        // text generator method, so it seems like we can just measure the game
        // object text w/ the IMGUI code
        
        private static readonly GUIStyle s_GUIStyle = new GUIStyle();
        private static readonly GUIContent s_GUIContent = new GUIContent();
        
        public float CalcTextWidth(string text, UIStyleSet style) {
            s_GUIStyle.font = style.font;
            s_GUIStyle.fontSize = style.fontSize;
            s_GUIStyle.fontStyle = style.fontStyle;
            s_GUIStyle.alignment = style.textAnchor;
            s_GUIStyle.richText = false;
            s_GUIContent.text = text;
            return Mathf.Ceil(s_GUIStyle.CalcSize(s_GUIContent).x) + 10f;
        }

        public float CalcTextHeight(string text, UIStyleSet style, float width) {
//            Debug.Log("Calculating Height " + width);
            s_GUIStyle.font = style.font;
            s_GUIStyle.fontSize = style.fontSize;
            s_GUIStyle.fontStyle = style.fontStyle;
            s_GUIStyle.alignment = style.textAnchor;
            s_GUIStyle.wordWrap = true;
            s_GUIStyle.richText = false;
            s_GUIContent.text = text;
            return Mathf.Ceil(s_GUIStyle.CalcHeight(s_GUIContent, width)) + 5f;
        }
        
    }

}